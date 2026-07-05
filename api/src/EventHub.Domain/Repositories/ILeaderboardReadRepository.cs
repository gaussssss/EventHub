using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

/// <summary>Classement des cœurs et statistiques communautaires agrégées.</summary>
public interface ILeaderboardReadRepository
{
    /// <summary>Top des utilisateurs par total de cœurs (rang 1-indexé, pagination skip/take).</summary>
    Task<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync(
        int skip, int take, CancellationToken cancellationToken = default);

    Task<CommunityStatsDto> GetCommunityStatsAsync(CancellationToken cancellationToken = default);
}
