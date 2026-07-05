namespace EventHub.Application.Leaderboard;

/// <summary>Ligne de classement exposée à l'app (avec drapeau « c'est moi »).</summary>
public sealed record LeaderboardRow(
    int Rank,
    string? Name,
    string? AvatarUrl,
    int Hearts,
    bool IsMe);
