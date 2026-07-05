using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

/// <summary>KPIs du tableau de bord (GET /api/admin/dashboard/overview).</summary>
public sealed record GetDashboardOverviewQuery : IQuery<DashboardOverviewDto>;
