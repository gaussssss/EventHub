using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Activities;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

/// <summary>Back office : mise à jour d'une activité (statut, « à la une », etc.).</summary>
public sealed class UpdateActivityHandler
    : ICommandHandler<UpdateActivityCommand, UpdateActivityStatus>
{
    private readonly IActivityRepository _activities;
    private readonly ICategoryRepository _categories;
    private readonly IClock _clock;

    public UpdateActivityHandler(
        IActivityRepository activities, ICategoryRepository categories, IClock clock)
    {
        _activities = activities;
        _categories = categories;
        _clock = clock;
    }

    public async Task<UpdateActivityStatus> HandleAsync(
        UpdateActivityCommand command, CancellationToken cancellationToken = default)
    {
        var activity = await _activities.GetAsync(command.Id, cancellationToken);
        if (activity is null)
            return UpdateActivityStatus.NotFound;

        if (!await _categories.ExistsAsync(command.CategoryId, cancellationToken))
            return UpdateActivityStatus.CategoryNotFound;

        activity.Update(
            command.Title, command.Description, command.CategoryId, command.OrganizerId,
            command.StartsAt, command.EndsAt, command.Location, command.ImageUrl,
            command.HeartsReward, command.MaxParticipants, command.RegistrationUrl,
            command.RegistrationDeadline, command.IsFeatured, command.Status, _clock.UtcNow,
            command.ParticipationCost);

        await _activities.SaveChangesAsync(cancellationToken);
        return UpdateActivityStatus.Updated;
    }
}
