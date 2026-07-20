using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class ContributorRepository : IContributorRepository
{
    private readonly EventHubDbContext _db;

    public ContributorRepository(EventHubDbContext db) => _db = db;

    public Task<Contributor?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Contributors.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Contributor contributor, CancellationToken cancellationToken = default) =>
        await _db.Contributors.AddAsync(contributor, cancellationToken);

    public void Remove(Contributor contributor) => _db.Contributors.Remove(contributor);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
