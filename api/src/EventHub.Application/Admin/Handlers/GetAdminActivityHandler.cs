using EventHub.Application.Common.Messaging;
using EventHub.Domain.ReadModels;
using EventHub.Domain.Repositories;

namespace EventHub.Application.Admin;

/// <summary>Renvoie le détail complet d'une activité (back office), ou null.</summary>
public sealed class GetAdminActivityHandler
    : IQueryHandler<GetAdminActivityQuery, AdminActivityDetailDto?>
{
    private readonly IActivityReadRepository _activities;

    public GetAdminActivityHandler(IActivityReadRepository activities) => _activities = activities;

    public Task<AdminActivityDetailDto?> HandleAsync(
        GetAdminActivityQuery query, CancellationToken cancellationToken = default) =>
        _activities.GetForAdminByIdAsync(query.Id, cancellationToken);
}
