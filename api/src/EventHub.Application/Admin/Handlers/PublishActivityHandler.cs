using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

public sealed class PublishActivityHandler
    : ICommandHandler<PublishActivityCommand, ActivityActionStatus>
{
    private readonly IActivityRepository _activities;
    private readonly IClock _clock;

    public PublishActivityHandler(IActivityRepository activities, IClock clock)
    {
        _activities = activities;
        _clock = clock;
    }

    public async Task<ActivityActionStatus> HandleAsync(
        PublishActivityCommand command, CancellationToken cancellationToken = default)
    {
        var activity = await _activities.GetAsync(command.Id, cancellationToken);
        if (activity is null)
            return ActivityActionStatus.NotFound;

        activity.Publish(_clock.UtcNow);
        await _activities.SaveChangesAsync(cancellationToken);
        return ActivityActionStatus.Done;
    }
}
