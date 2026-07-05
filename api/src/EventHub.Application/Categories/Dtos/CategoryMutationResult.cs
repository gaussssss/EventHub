using EventHub.Application.Common.Results;

namespace EventHub.Application.Categories;

/// <summary>Résultat d'une création de catégorie (issue + id créé).</summary>
public sealed record CategoryMutationResult(CrudOutcome Outcome, Guid Id);
