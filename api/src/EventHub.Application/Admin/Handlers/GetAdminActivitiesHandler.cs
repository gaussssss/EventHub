using EventHub.Application.Common.Messaging;
using EventHub.Domain.ReadModels;
using EventHub.Domain.Repositories;

namespace EventHub.Application.Admin;

/// <summary>Renvoie toutes les activités (tous statuts) pour le back office.</summary>
public sealed class GetAdminActivitiesHandler
    : IQueryHandler<GetAdminActivitiesQuery, IReadOnlyList<AdminActivityDto>>
{
    private readonly IActivityReadRepository _activities;

    public GetAdminActivitiesHandler(IActivityReadRepository activities) => _activities = activities;

    public Task<IReadOnlyList<AdminActivityDto>> HandleAsync(
        GetAdminActivitiesQuery query, CancellationToken cancellationToken = default) =>
        _activities.GetAllForAdminAsync(cancellationToken);
}
