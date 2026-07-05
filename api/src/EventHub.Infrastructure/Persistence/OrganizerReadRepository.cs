using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class OrganizerReadRepository : IOrganizerReadRepository
{
    private readonly EventHubDbContext _db;

    public OrganizerReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrganizerDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await _db.Organizers.AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new OrganizerDto
            {
                Id = o.Id,
                Name = o.Name,
                ContactEmail = o.ContactEmail,
            })
            .ToListAsync(cancellationToken);
}
