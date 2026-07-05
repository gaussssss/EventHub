using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Categories;

public sealed class CreateCategoryHandler
    : ICommandHandler<CreateCategoryCommand, CategoryMutationResult>
{
    private readonly ICategoryRepository _categories;

    public CreateCategoryHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<CategoryMutationResult> HandleAsync(
        CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (await _categories.SlugExistsAsync(command.Slug, null, cancellationToken))
            return new CategoryMutationResult(CrudOutcome.Conflict, Guid.Empty);

        var category = Category.Create(command.Slug, command.Label, command.Color, command.Icon);
        await _categories.AddAsync(category, cancellationToken);
        await _categories.SaveChangesAsync(cancellationToken);
        return new CategoryMutationResult(CrudOutcome.Done, category.Id);
    }
}
