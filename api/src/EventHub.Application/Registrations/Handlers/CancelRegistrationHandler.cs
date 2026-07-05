using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Enums;

namespace EventHub.Application.Registrations;

/// <summary>
/// Cas d'usage « annuler son inscription ». Si l'inscription occupait une place,
/// promeut automatiquement le premier de la liste d'attente et diffuse les
/// changements en temps réel.
/// </summary>
public sealed class CancelRegistrationHandler
    : ICommandHandler<CancelRegistrationCommand, CancellationResult>
{
    private readonly IRegistrationRepository _registrations;
    private readonly IRealtimeNotifier _notifier;
    private readonly IClock _clock;

    public CancelRegistrationHandler(
        IRegistrationRepository registrations,
        IRealtimeNotifier notifier,
        IClock clock)
    {
        _registrations = registrations;
        _notifier = notifier;
        _clock = clock;
    }

    public async Task<CancellationResult> HandleAsync(
        CancelRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        var (userId, activityId) = command;
        var registration = await _registrations.FindAsync(userId, activityId, cancellationToken);
        if (registration is null || registration.Status is
            RegistrationStatus.Cancelled or RegistrationStatus.NoShow)
        {
            return new CancellationResult(CancellationResultStatus.NotRegistered);
        }

        var freedSpot = registration.OccupiesSpot;
        registration.Cancel(_clock.UtcNow);

        Guid? promotedUserId = null;
        if (freedSpot)
        {
            var next = await _registrations.FindFirstWaitlistedAsync(activityId, cancellationToken);
            if (next is not null)
            {
                next.PromoteFromWaitlist(_clock.UtcNow);
                promotedUserId = next.UserId;
            }
        }

        await _registrations.SaveChangesAsync(cancellationToken);

        if (freedSpot)
        {
            var current = await _registrations.CountActiveAsync(activityId, cancellationToken);
            await _notifier.ActivityParticipantsChangedAsync(activityId, current, cancellationToken);

            if (promotedUserId is not null)
                await _notifier.RegistrationPromotedAsync(
                    promotedUserId.Value, activityId, cancellationToken);
        }

        return new CancellationResult(CancellationResultStatus.Cancelled, promotedUserId);
    }
}
