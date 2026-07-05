using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

public sealed class GetDashboardOverviewHandler
    : IQueryHandler<GetDashboardOverviewQuery, DashboardOverviewDto>
{
    private readonly IDashboardReadRepository _dashboard;
    private readonly IClock _clock;

    public GetDashboardOverviewHandler(IDashboardReadRepository dashboard, IClock clock)
    {
        _dashboard = dashboard;
        _clock = clock;
    }

    public Task<DashboardOverviewDto> HandleAsync(
        GetDashboardOverviewQuery query, CancellationToken cancellationToken = default) =>
        _dashboard.GetOverviewAsync(_clock.UtcNow, cancellationToken);
}
