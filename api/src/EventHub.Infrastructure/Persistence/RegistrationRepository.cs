using EventHub.Application.Common.Exceptions;
using EventHub.Application.Registrations;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class RegistrationRepository : IRegistrationRepository
{
    private readonly EventHubDbContext _db;

    public RegistrationRepository(EventHubDbContext db) => _db = db;

    public Task<int> CountActiveAsync(Guid activityId, CancellationToken cancellationToken = default) =>
        _db.Registrations.CountAsync(
            r => r.ActivityId == activityId &&
                 (r.Status == RegistrationStatus.Registered ||
                  r.Status == RegistrationStatus.Attended),
            cancellationToken);

    public Task<Registration?> FindAsync(
        Guid userId, Guid activityId, CancellationToken cancellationToken = default) =>
        _db.Registrations.FirstOrDefaultAsync(
            r => r.UserId == userId && r.ActivityId == activityId, cancellationToken);

    public Task<Registration?> FindFirstWaitlistedAsync(
        Guid activityId, CancellationToken cancellationToken = default) =>
        _db.Registrations
            .Where(r => r.ActivityId == activityId && r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.RegisteredAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Registration>> GetForActivityAsync(
        Guid activityId, IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.ToList();
        return await _db.Registrations
            .Where(r => r.ActivityId == activityId && ids.Contains(r.UserId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetActiveActivityIdsAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Registrations
            .Where(r => r.UserId == userId &&
                        (r.Status == RegistrationStatus.Registered ||
                         r.Status == RegistrationStatus.Attended))
            .Select(r => r.ActivityId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Registration registration, CancellationToken cancellationToken = default) =>
        await _db.Registrations.AddAsync(registration, cancellationToken);

    /// <summary>
    /// Persiste l'unité de travail. Traduit un conflit de concurrence optimiste EF
    /// (jeton <c>Activity.Version</c>) en <see cref="ConcurrencyConflictException"/>
    /// après avoir rechargé les entités en conflit, pour que le handler rejoue.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
                await entry.ReloadAsync(cancellationToken);
            throw new ConcurrencyConflictException();
        }
    }
}
