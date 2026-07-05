using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

/// <summary>
/// Agrégats de gamification : classement (SUM des cœurs par utilisateur, join
/// sur l'identité) et statistiques communautaires. Calculés à la volée.
/// </summary>
public sealed class LeaderboardReadRepository : ILeaderboardReadRepository
{
    private readonly EventHubDbContext _db;

    public LeaderboardReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync(
        int skip, int take, CancellationToken cancellationToken = default)
    {
        var sums = _db.HeartTransactions
            .GroupBy(h => h.UserId)
            .Select(g => new { UserId = g.Key, Hearts = g.Sum(x => x.Hearts) });

        var ranked = await sums
            .Join(_db.Users,
                s => s.UserId,
                u => u.Id,
                (s, u) => new { u.Id, u.Name, u.AvatarUrl, s.Hearts })
            .OrderByDescending(x => x.Hearts)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return ranked
            .Select((r, i) => new LeaderboardEntryDto
            {
                Rank = skip + i + 1,
                UserId = r.Id,
                Name = r.Name,
                AvatarUrl = r.AvatarUrl,
                Hearts = r.Hearts,
            })
            .ToList();
    }

    public async Task<CommunityStatsDto> GetCommunityStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var totalUsers = await _db.Users.CountAsync(cancellationToken);
        var totalHearts = await _db.HeartTransactions
            .SumAsync(h => (long)h.Hearts, cancellationToken);

        return new CommunityStatsDto(totalUsers, totalHearts);
    }
}
