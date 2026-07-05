using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly EventHubDbContext _db;

    public CategoryRepository(EventHubDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Categories.AnyAsync(c => c.Id == id, cancellationToken);

    public Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(
        string slug, Guid? excludeId = null, CancellationToken cancellationToken = default) =>
        _db.Categories.AnyAsync(
            c => c.Slug == slug && (excludeId == null || c.Id != excludeId), cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default) =>
        await _db.Categories.AddAsync(category, cancellationToken);

    public void Remove(Category category) => _db.Categories.Remove(category);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
