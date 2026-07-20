using EventHub.Domain.ReadModels;
using EventHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class ContributorReadRepository : IContributorReadRepository
{
    private readonly EventHubDbContext _db;

    public ContributorReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<ContributorDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Contributors.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ContributorDto
            {
                Id = c.Id,
                Name = c.Name,
                Role = c.Role,
                AvatarUrl = c.AvatarUrl,
                SortOrder = c.SortOrder,
            })
            .ToListAsync(cancellationToken);
    }
}
