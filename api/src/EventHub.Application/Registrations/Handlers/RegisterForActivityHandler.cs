using EventHub.Domain.Services;
using EventHub.Application.Common.Exceptions;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Repositories;

namespace EventHub.Application.Registrations;

/// <summary>
/// Cas d'usage « s'inscrire à une activité ». Applique la règle domaine
/// <see cref="Activity.DetermineOutcome"/> (capacité + échéance + statut),
/// persiste l'inscription, et diffuse le changement de participants en temps réel.
/// Idempotent : une inscription active existante n'est pas dupliquée.
///
/// Concurrence : la prise de place incrémente le jeton <see cref="Activity.Version"/>
/// (via <see cref="Activity.ClaimSpot"/>). Deux demandes concurrentes sur la dernière
/// place entrent en conflit ; la perdante est <b>rejouée</b> et bascule en liste
/// d'attente — jamais de sur-réservation.
/// </summary>
public sealed class RegisterForActivityHandler
    : ICommandHandler<RegisterForActivityCommand, RegistrationResult>
{
    private const int MaxAttempts = 3;

    private readonly IActivityRepository _activities;
    private readonly IRegistrationRepository _registrations;
    private readonly IRealtimeNotifier _notifier;
    private readonly IClock _clock;

    public RegisterForActivityHandler(
        IActivityRepository activities,
        IRegistrationRepository registrations,
        IRealtimeNotifier notifier,
        IClock clock)
    {
        _activities = activities;
        _registrations = registrations;
        _notifier = notifier;
        _clock = clock;
    }

    public async Task<RegistrationResult> HandleAsync(
        RegisterForActivityCommand command, CancellationToken cancellationToken = default)
    {
        var (userId, activityId, formResponseId) = command;

        Registration? pending = null;
        var added = false;

        for (var attempt = 1; ; attempt++)
        {
            var activity = await _activities.GetAsync(activityId, cancellationToken);
            if (activity is null)
                return new RegistrationResult(RegistrationResultStatus.ActivityNotFound);

            var existing = await _registrations.FindAsync(userId, activityId, cancellationToken);
            if (existing?.Status is RegistrationStatus.Registered
                or RegistrationStatus.Waitlisted
                or RegistrationStatus.Attended)
            {
                return new RegistrationResult(RegistrationResultStatus.AlreadyRegistered);
            }

            var current = await _registrations.CountActiveAsync(activityId, cancellationToken);
            var outcome = activity.DetermineOutcome(current, _clock.UtcNow);
            if (!outcome.IsAccepted)
                return new RegistrationResult(RegistrationResultStatus.Rejected, outcome.Reason);

            var status = outcome.Status!.Value;

            if (existing is not null)
            {
                existing.AssignOutcome(status, Registration.SourceApp, formResponseId, _clock.UtcNow);
            }
            else
            {
                pending ??= Registration.Create(
                    userId, activityId, status, Registration.SourceApp, formResponseId, _clock.UtcNow);
                pending.AssignOutcome(status, Registration.SourceApp, formResponseId, _clock.UtcNow);
                if (!added)
                {
                    await _registrations.AddAsync(pending, cancellationToken);
                    added = true;
                }
            }

            // Consommer une place touche la ligne Activity → sérialise la dernière place.
            if (status == RegistrationStatus.Registered)
                activity.ClaimSpot(_clock.UtcNow);

            try
            {
                await _registrations.SaveChangesAsync(cancellationToken);
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                continue; // l'Activity a été rechargée : on réévalue capacité + statut.
            }

            if (status == RegistrationStatus.Registered)
            {
                await _notifier.ActivityParticipantsChangedAsync(
                    activityId, current + 1, cancellationToken);
                return new RegistrationResult(RegistrationResultStatus.Registered);
            }

            return new RegistrationResult(RegistrationResultStatus.Waitlisted);
        }
    }
}
