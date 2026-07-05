using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Categories;

/// <summary>Mettre à jour une catégorie (PATCH /api/admin/categories/{id}).</summary>
public sealed record UpdateCategoryCommand(
    Guid Id, string Slug, string Label, string? Color, string? Icon)
    : ICommand<CrudOutcome>;
