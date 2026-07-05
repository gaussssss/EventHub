namespace EventHub.Domain.ReadModels;

/// <summary>Une ligne du classement (GET /api/leaderboard). Le rang est 1-indexé.</summary>
public sealed record LeaderboardEntryDto
{
    public required int Rank { get; init; }
    public required Guid UserId { get; init; }
    public string? Name { get; init; }
    public string? AvatarUrl { get; init; }
    public int Hearts { get; init; }
}
