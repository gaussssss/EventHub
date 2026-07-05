using EventHub.Application.Common.Messaging;
using EventHub.Domain.ReadModels;

namespace EventHub.Application.Admin;

/// <summary>Liste des activités (tous statuts) pour le back office.</summary>
public sealed record GetAdminActivitiesQuery : IQuery<IReadOnlyList<AdminActivityDto>>;
