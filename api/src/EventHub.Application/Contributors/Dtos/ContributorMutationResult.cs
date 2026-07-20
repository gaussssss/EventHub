using EventHub.Application.Common.Results;

namespace EventHub.Application.Contributors;

/// <summary>Résultat d'une écriture sur un contributeur (issue + id).</summary>
public sealed record ContributorMutationResult(CrudOutcome Outcome, Guid Id);
