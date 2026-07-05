using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

public sealed class ToggleFeatureHandler
    : ICommandHandler<ToggleFeatureCommand, ToggleFeatureResult>
{
    private readonly IActivityRepository _activities;
    private readonly IClock _clock;

    public ToggleFeatureHandler(IActivityRepository activities, IClock clock)
    {
        _activities = activities;
        _clock = clock;
    }

    public async Task<ToggleFeatureResult> HandleAsync(
        ToggleFeatureCommand command, CancellationToken cancellationToken = default)
    {
        var activity = await _activities.GetAsync(command.Id, cancellationToken);
        if (activity is null)
            return new ToggleFeatureResult(ActivityActionStatus.NotFound, false);

        var featured = activity.ToggleFeatured(_clock.UtcNow);
        await _activities.SaveChangesAsync(cancellationToken);
        return new ToggleFeatureResult(ActivityActionStatus.Done, featured);
    }
}
