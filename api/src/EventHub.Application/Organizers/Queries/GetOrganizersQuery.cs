using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Organizers;

/// <summary>Liste des organisateurs (GET /api/admin/organizers).</summary>
public sealed record GetOrganizersQuery : IQuery<IReadOnlyList<OrganizerDto>>;
