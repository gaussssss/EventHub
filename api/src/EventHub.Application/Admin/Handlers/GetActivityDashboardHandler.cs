using EventHub.Application.Common.Messaging;
using EventHub.Domain.ReadModels;
using EventHub.Domain.Repositories;

namespace EventHub.Application.Admin;

/// <summary>Renvoie les statistiques d'une activité (back office), ou null.</summary>
public sealed class GetActivityDashboardHandler
    : IQueryHandler<GetActivityDashboardQuery, ActivityDashboardDto?>
{
    private readonly IDashboardReadRepository _dashboard;

    public GetActivityDashboardHandler(IDashboardReadRepository dashboard) => _dashboard = dashboard;

    public Task<ActivityDashboardDto?> HandleAsync(
        GetActivityDashboardQuery query, CancellationToken cancellationToken = default) =>
        _dashboard.GetActivityDashboardAsync(query.ActivityId, cancellationToken);
}
