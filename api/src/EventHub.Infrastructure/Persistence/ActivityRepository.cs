using EventHub.Domain.Repositories;
using EventHub.Application.Activities;
using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class ActivityRepository : IActivityRepository
{
    private readonly EventHubDbContext _db;

    public ActivityRepository(EventHubDbContext db) => _db = db;

    public Task<Activity?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Activities.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Activity activity, CancellationToken cancellationToken = default) =>
        await _db.Activities.AddAsync(activity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
