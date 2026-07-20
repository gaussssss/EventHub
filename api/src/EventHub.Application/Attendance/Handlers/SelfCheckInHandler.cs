using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;

namespace EventHub.Application.Attendance;

/// <summary>
/// Cas d'usage « je scanne le QR de l'événement pour confirmer ma présence ».
/// Sécurité : le jeton scanné doit correspondre au <see cref="Activity.CheckInToken"/>,
/// l'utilisateur doit être inscrit, et le scan doit tomber dans la fenêtre horaire
/// de l'événement. Crédite les cœurs comme la prise de présence back-office, sans
/// double crédit.
/// </summary>
public sealed class SelfCheckInHandler
    : ICommandHandler<SelfCheckInCommand, SelfCheckInResult>
{
    // Tolérance autour de l'événement pendant laquelle l'émargement est ouvert.
    private static readonly TimeSpan OpenBefore = TimeSpan.FromHours(2);
    private static readonly TimeSpan OpenAfter = TimeSpan.FromHours(2);
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(4);

    private readonly IActivityRepository _activities;
    private readonly IRegistrationRepository _registrations;
    private readonly IHeartTransactionRepository _hearts;
    private readonly IClock _clock;

    public SelfCheckInHandler(
        IActivityRepository activities,
        IRegistrationRepository registrations,
        IHeartTransactionRepository hearts,
        IClock clock)
    {
        _activities = activities;
        _registrations = registrations;
        _hearts = hearts;
        _clock = clock;
    }

    public async Task<SelfCheckInResult> HandleAsync(
        SelfCheckInCommand command, CancellationToken cancellationToken = default)
    {
        var activity = await _activities.GetAsync(command.ActivityId, cancellationToken);
        if (activity is null)
            return new SelfCheckInResult(SelfCheckInStatus.ActivityNotFound, 0, false);

        // Jeton du QR ⇔ jeton secret de l'activité.
        if (command.Token == Guid.Empty || activity.CheckInToken != command.Token)
            return new SelfCheckInResult(SelfCheckInStatus.InvalidToken, 0, false);

        // Fenêtre horaire (évite les scans très en avance / longtemps après).
        var now = _clock.UtcNow;
        var opens = activity.StartsAt - OpenBefore;
        var closes = (activity.EndsAt ?? activity.StartsAt + DefaultDuration) + OpenAfter;
        if (now < opens || now > closes)
            return new SelfCheckInResult(SelfCheckInStatus.OutsideWindow, 0, false);

        // L'utilisateur doit être inscrit (non annulé).
        var registration =
            await _registrations.FindAsync(command.UserId, command.ActivityId, cancellationToken);
        if (registration is null || registration.Status == RegistrationStatus.Cancelled)
            return new SelfCheckInResult(SelfCheckInStatus.NotRegistered, 0, false);

        var alreadyCredited = await _hearts.HasAttendanceCreditAsync(
            command.UserId, command.ActivityId, cancellationToken);

        if (registration.Status != RegistrationStatus.Attended)
            registration.MarkAttended(now);

        var credited = 0;
        if (!alreadyCredited && activity.HeartsReward > 0)
        {
            await _hearts.AddAsync(HeartTransaction.ForAttendance(
                command.UserId, command.ActivityId, activity.Title,
                activity.HeartsReward, now), cancellationToken);
            credited = activity.HeartsReward;
        }

        await _hearts.SaveChangesAsync(cancellationToken);
        return new SelfCheckInResult(SelfCheckInStatus.Ok, credited, alreadyCredited);
    }
}
