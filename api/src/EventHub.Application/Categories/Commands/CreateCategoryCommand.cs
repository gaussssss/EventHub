using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Categories;

/// <summary>Créer une catégorie (POST /api/admin/categories).</summary>
public sealed record CreateCategoryCommand(
    string Slug, string Label, string? Color, string? Icon)
    : ICommand<CategoryMutationResult>;
