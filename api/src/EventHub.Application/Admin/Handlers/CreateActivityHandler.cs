using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Activities;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Entities;

namespace EventHub.Application.Admin;

/// <summary>Back office : création d'une activité.</summary>
public sealed class CreateActivityHandler
    : ICommandHandler<CreateActivityCommand, CreateActivityResult>
{
    private readonly IActivityRepository _activities;
    private readonly ICategoryRepository _categories;
    private readonly IClock _clock;

    public CreateActivityHandler(
        IActivityRepository activities, ICategoryRepository categories, IClock clock)
    {
        _activities = activities;
        _categories = categories;
        _clock = clock;
    }

    public async Task<CreateActivityResult> HandleAsync(
        CreateActivityCommand command, CancellationToken cancellationToken = default)
    {
        if (!await _categories.ExistsAsync(command.CategoryId, cancellationToken))
            return new CreateActivityResult(CreateActivityStatus.CategoryNotFound);

        var activity = Activity.Create(
            command.Title, command.Description, command.CategoryId, command.OrganizerId,
            command.StartsAt, command.EndsAt, command.Location, command.ImageUrl,
            command.HeartsReward, command.MaxParticipants, command.RegistrationUrl,
            command.RegistrationDeadline, command.IsFeatured, command.Status, _clock.UtcNow);

        await _activities.AddAsync(activity, cancellationToken);
        await _activities.SaveChangesAsync(cancellationToken);
        return new CreateActivityResult(CreateActivityStatus.Created, activity.Id);
    }
}
