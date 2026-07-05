using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Stats;

/// <summary>Statistiques communautaires (GET /api/stats/community).</summary>
public sealed record GetCommunityStatsQuery : IQuery<CommunityStatsDto>;
