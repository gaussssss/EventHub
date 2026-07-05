using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Categories;

public sealed class UpdateCategoryHandler : ICommandHandler<UpdateCategoryCommand, CrudOutcome>
{
    private readonly ICategoryRepository _categories;

    public UpdateCategoryHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<CrudOutcome> HandleAsync(
        UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetAsync(command.Id, cancellationToken);
        if (category is null)
            return CrudOutcome.NotFound;

        if (await _categories.SlugExistsAsync(command.Slug, command.Id, cancellationToken))
            return CrudOutcome.Conflict;

        category.Update(command.Slug, command.Label, command.Color, command.Icon);
        await _categories.SaveChangesAsync(cancellationToken);
        return CrudOutcome.Done;
    }
}
