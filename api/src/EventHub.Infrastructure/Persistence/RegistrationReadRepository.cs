using EventHub.Domain.Enums;
using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

/// <summary>Projection des inscriptions d'une activité (join sur l'identité) pour le back office.</summary>
public sealed class RegistrationReadRepository : IRegistrationReadRepository
{
    private readonly EventHubDbContext _db;

    public RegistrationReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<RegistrationEntryDto>> GetByActivityAsync(
        Guid activityId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.Registrations
            .AsNoTracking()
            .Where(r => r.ActivityId == activityId && r.Status != RegistrationStatus.Cancelled)
            .Join(_db.Users,
                r => r.UserId,
                u => u.Id,
                (r, u) => new { r.UserId, u.Name, u.Email, r.Status, r.RegisteredAt })
            .OrderBy(r => r.Status)
            .ThenBy(r => r.RegisteredAt)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new RegistrationEntryDto
            {
                UserId = r.UserId,
                Name = r.Name,
                Email = r.Email,
                Status = r.Status.ToString().ToLowerInvariant(),
                RegisteredAt = r.RegisteredAt,
            })
            .ToList();
    }
}
