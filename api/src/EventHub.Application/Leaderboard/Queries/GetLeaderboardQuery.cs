using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Leaderboard;

/// <summary>Classement global des cœurs (GET /api/leaderboard).</summary>
public sealed record GetLeaderboardQuery(Guid? CurrentUserId, int Page = 1, int PageSize = 50)
    : IQuery<IReadOnlyList<LeaderboardRow>>;
