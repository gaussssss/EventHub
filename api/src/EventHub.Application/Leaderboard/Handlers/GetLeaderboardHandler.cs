using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Leaderboard;

public sealed class GetLeaderboardHandler
    : IQueryHandler<GetLeaderboardQuery, IReadOnlyList<LeaderboardRow>>
{
    private readonly ILeaderboardReadRepository _leaderboard;

    public GetLeaderboardHandler(ILeaderboardReadRepository leaderboard) =>
        _leaderboard = leaderboard;

    public async Task<IReadOnlyList<LeaderboardRow>> HandleAsync(
        GetLeaderboardQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var size = Math.Clamp(query.PageSize, 1, 100);

        var entries = await _leaderboard.GetTopAsync(
            (page - 1) * size, size, cancellationToken);

        return entries
            .Select(e => new LeaderboardRow(
                e.Rank, e.Name, e.AvatarUrl, e.Hearts,
                IsMe: query.CurrentUserId is not null && e.UserId == query.CurrentUserId))
            .ToList();
    }
}
