using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Organizers;

/// <summary>Créer un organisateur (POST /api/admin/organizers).</summary>
public sealed record CreateOrganizerCommand(string Name, string? ContactEmail)
    : ICommand<OrganizerMutationResult>;
