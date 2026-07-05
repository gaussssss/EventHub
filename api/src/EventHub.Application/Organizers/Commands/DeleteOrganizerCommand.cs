using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Organizers;

/// <summary>Supprimer un organisateur (DELETE /api/admin/organizers/{id}).</summary>
public sealed record DeleteOrganizerCommand(Guid Id) : ICommand<CrudOutcome>;
