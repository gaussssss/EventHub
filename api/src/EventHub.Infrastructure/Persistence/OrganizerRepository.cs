using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class OrganizerRepository : IOrganizerRepository
{
    private readonly EventHubDbContext _db;

    public OrganizerRepository(EventHubDbContext db) => _db = db;

    public Task<Organizer?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Organizers.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(Organizer organizer, CancellationToken cancellationToken = default) =>
        await _db.Organizers.AddAsync(organizer, cancellationToken);

    public void Remove(Organizer organizer) => _db.Organizers.Remove(organizer);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
