using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Stats;

public sealed class GetCommunityStatsHandler
    : IQueryHandler<GetCommunityStatsQuery, CommunityStatsDto>
{
    private readonly ILeaderboardReadRepository _leaderboard;

    public GetCommunityStatsHandler(ILeaderboardReadRepository leaderboard) =>
        _leaderboard = leaderboard;

    public Task<CommunityStatsDto> HandleAsync(
        GetCommunityStatsQuery query, CancellationToken cancellationToken = default) =>
        _leaderboard.GetCommunityStatsAsync(cancellationToken);
}
