using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Organizers;

/// <summary>Mettre à jour un organisateur (PATCH /api/admin/organizers/{id}).</summary>
public sealed record UpdateOrganizerCommand(Guid Id, string Name, string? ContactEmail)
    : ICommand<CrudOutcome>;
