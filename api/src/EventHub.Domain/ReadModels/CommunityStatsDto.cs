namespace EventHub.Domain.ReadModels;

/// <summary>Badges d'accueil (GET /api/stats/community).</summary>
public sealed record CommunityStatsDto(int TotalRegisteredUsers, long TotalUqtrHearts);
