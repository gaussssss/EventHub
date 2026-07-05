using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Categories;

/// <summary>Supprimer une catégorie (DELETE /api/admin/categories/{id}).</summary>
public sealed record DeleteCategoryCommand(Guid Id) : ICommand<CrudOutcome>;
