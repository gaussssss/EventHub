using EventHub.Domain.Repositories;
using EventHub.Application.Hearts;
using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class HeartTransactionRepository : IHeartTransactionRepository
{
    private readonly EventHubDbContext _db;

    public HeartTransactionRepository(EventHubDbContext db) => _db = db;

    public Task<bool> HasAttendanceCreditAsync(
        Guid userId, Guid activityId, CancellationToken cancellationToken = default) =>
        _db.HeartTransactions.AnyAsync(
            h => h.UserId == userId &&
                 h.ActivityId == activityId &&
                 h.Reason == "attendance",
            cancellationToken);

    public async Task AddAsync(
        HeartTransaction transaction, CancellationToken cancellationToken = default) =>
        await _db.HeartTransactions.AddAsync(transaction, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
