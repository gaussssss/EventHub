using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

/// <summary>Projection EF Core du référentiel des catégories vers <see cref="CategoryDto"/>.</summary>
public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private readonly EventHubDbContext _db;

    public CategoryReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Label)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Slug = c.Slug,
                Label = c.Label,
                Color = c.Color,
                Icon = c.Icon,
            })
            .ToListAsync(cancellationToken);
}
