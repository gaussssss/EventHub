using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Hearts;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class HeartReadRepository : IHeartReadRepository
{
    private readonly EventHubDbContext _db;

    public HeartReadRepository(EventHubDbContext db) => _db = db;

    public Task<int> GetTotalAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.HeartTransactions
            .Where(h => h.UserId == userId)
            .SumAsync(h => h.Hearts, cancellationToken);

    public async Task<IReadOnlyList<HeartHistoryDto>> GetHistoryAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await _db.HeartTransactions
            .AsNoTracking()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new HeartHistoryDto(
                h.ActivityTitle ?? string.Empty, h.Hearts, h.CreatedAt))
            .ToListAsync(cancellationToken);
}
