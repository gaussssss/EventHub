using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Activities;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Hearts;
using EventHub.Application.Registrations;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;

namespace EventHub.Application.Attendance;

/// <summary>
/// Cas d'usage « marquer les présences » (back office). C'est ICI que les cœurs
/// sont attribués — à la présence confirmée, pas à la simple inscription
/// (voir docs/BACKEND_MANIFEST.md §7). Idempotent : pas de double crédit.
/// </summary>
public sealed class MarkAttendanceHandler
    : ICommandHandler<MarkAttendanceCommand, AttendanceResult>
{
    private readonly IActivityRepository _activities;
    private readonly IRegistrationRepository _registrations;
    private readonly IHeartTransactionRepository _hearts;
    private readonly IClock _clock;

    public MarkAttendanceHandler(
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

    public async Task<AttendanceResult> HandleAsync(
        MarkAttendanceCommand command, CancellationToken cancellationToken = default)
    {
        var (activityId, userIds) = command;
        var activity = await _activities.GetAsync(activityId, cancellationToken);
        if (activity is null)
            return new AttendanceResult(AttendanceResultStatus.ActivityNotFound, 0);

        var registrations =
            await _registrations.GetForActivityAsync(activityId, userIds, cancellationToken);

        var credited = 0;
        foreach (var registration in registrations)
        {
            if (registration.Status is RegistrationStatus.Cancelled or RegistrationStatus.NoShow)
                continue;

            registration.MarkAttended(_clock.UtcNow);

            var alreadyCredited = await _hearts.HasAttendanceCreditAsync(
                registration.UserId, activityId, cancellationToken);

            if (!alreadyCredited && activity.HeartsReward > 0)
            {
                await _hearts.AddAsync(HeartTransaction.ForAttendance(
                    registration.UserId, activityId, activity.Title,
                    activity.HeartsReward, _clock.UtcNow), cancellationToken);
                credited++;
            }
        }

        // Un seul SaveChanges : le DbContext partagé persiste les changements de
        // statut d'inscription ET les nouvelles transactions de cœurs.
        await _hearts.SaveChangesAsync(cancellationToken);

        return new AttendanceResult(AttendanceResultStatus.Ok, credited);
    }
}
