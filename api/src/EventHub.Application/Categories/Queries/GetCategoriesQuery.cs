using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Categories;

/// <summary>Requête « référentiel des catégories » (GET /api/categories).</summary>
public sealed record GetCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>;
