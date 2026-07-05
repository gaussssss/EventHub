using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

/// <summary>Inscrits + liste d'attente d'une activité (GET /api/admin/activities/{id}/registrations).</summary>
public sealed record GetActivityRegistrationsQuery(Guid ActivityId)
    : IQuery<IReadOnlyList<RegistrationEntryDto>>;
