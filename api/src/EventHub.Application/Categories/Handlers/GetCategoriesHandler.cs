using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Categories;

public sealed class GetCategoriesHandler
    : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryReadRepository _categories;

    public GetCategoriesHandler(ICategoryReadRepository categories) => _categories = categories;

    public Task<IReadOnlyList<CategoryDto>> HandleAsync(
        GetCategoriesQuery query, CancellationToken cancellationToken = default) =>
        _categories.GetAllAsync(cancellationToken);
}
