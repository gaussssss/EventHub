using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Activities;

public sealed class GetActivityByIdHandler : IQueryHandler<GetActivityByIdQuery, ActivityDto?>
{
    private readonly IActivityReadRepository _activities;

    public GetActivityByIdHandler(IActivityReadRepository activities) => _activities = activities;

    public Task<ActivityDto?> HandleAsync(
        GetActivityByIdQuery query, CancellationToken cancellationToken = default) =>
        _activities.GetByIdAsync(query.Id, cancellationToken);
}
