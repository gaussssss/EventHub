using EventHub.Application.Common.Results;

namespace EventHub.Application.Organizers;

/// <summary>Résultat d'une création d'organisateur (issue + id créé).</summary>
public sealed record OrganizerMutationResult(CrudOutcome Outcome, Guid Id);
