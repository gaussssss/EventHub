using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(
        string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    void Remove(Category category);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
