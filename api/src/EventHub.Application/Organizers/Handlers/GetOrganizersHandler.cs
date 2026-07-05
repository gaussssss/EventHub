using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Organizers;

public sealed class GetOrganizersHandler
    : IQueryHandler<GetOrganizersQuery, IReadOnlyList<OrganizerDto>>
{
    private readonly IOrganizerReadRepository _organizers;

    public GetOrganizersHandler(IOrganizerReadRepository organizers) => _organizers = organizers;

    public Task<IReadOnlyList<OrganizerDto>> HandleAsync(
        GetOrganizersQuery query, CancellationToken cancellationToken = default) =>
        _organizers.GetAllAsync(cancellationToken);
}
