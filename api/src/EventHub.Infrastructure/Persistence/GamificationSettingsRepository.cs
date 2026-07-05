using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class GamificationSettingsRepository : IGamificationSettingsRepository
{
    private readonly EventHubDbContext _db;

    public GamificationSettingsRepository(EventHubDbContext db) => _db = db;

    public Task<GamificationSettings?> GetAsync(CancellationToken cancellationToken = default) =>
        _db.GamificationSettings.FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(
        GamificationSettings settings, CancellationToken cancellationToken = default) =>
        await _db.GamificationSettings.AddAsync(settings, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
