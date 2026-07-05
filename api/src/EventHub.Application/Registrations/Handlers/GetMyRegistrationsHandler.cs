using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Registrations;

public sealed class GetMyRegistrationsHandler
    : IQueryHandler<GetMyRegistrationsQuery, IReadOnlyList<ActivityDto>>
{
    private readonly IActivityReadRepository _activities;

    public GetMyRegistrationsHandler(IActivityReadRepository activities) => _activities = activities;

    public Task<IReadOnlyList<ActivityDto>> HandleAsync(
        GetMyRegistrationsQuery query, CancellationToken cancellationToken = default) =>
        _activities.GetRegisteredByUserAsync(query.UserId, cancellationToken);
}
