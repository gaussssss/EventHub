using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Activities;

public sealed class GetActivitiesHandler
    : IQueryHandler<GetActivitiesQuery, IReadOnlyList<ActivityDto>>
{
    private readonly IActivityReadRepository _activities;

    public GetActivitiesHandler(IActivityReadRepository activities) => _activities = activities;

    public Task<IReadOnlyList<ActivityDto>> HandleAsync(
        GetActivitiesQuery query, CancellationToken cancellationToken = default) =>
        _activities.GetActivitiesAsync(query.Filter, cancellationToken);
}
