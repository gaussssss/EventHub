using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Categories;

public sealed class DeleteCategoryHandler : ICommandHandler<DeleteCategoryCommand, CrudOutcome>
{
    private readonly ICategoryRepository _categories;

    public DeleteCategoryHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<CrudOutcome> HandleAsync(
        DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetAsync(command.Id, cancellationToken);
        if (category is null)
            return CrudOutcome.NotFound;

        _categories.Remove(category);
        await _categories.SaveChangesAsync(cancellationToken);
        return CrudOutcome.Done;
    }
}
