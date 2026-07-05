using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Registrations;

/// <summary>Activités inscrites de l'utilisateur courant (GET /api/me/registrations).</summary>
public sealed record GetMyRegistrationsQuery(Guid UserId)
    : IQuery<IReadOnlyList<ActivityDto>>;
