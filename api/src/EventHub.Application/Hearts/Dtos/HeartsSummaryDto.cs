using EventHub.Domain.ReadModels;

namespace EventHub.Application.Hearts;

/// <summary>
/// Résumé « cœurs santé » d'un utilisateur (GET /me/hearts). Le niveau et les
/// seuils sont calculés côté serveur via <c>HeartLevel</c>.
/// </summary>
public sealed record HeartsSummaryDto
{
    public required int TotalHearts { get; init; }
    public required string Level { get; init; }
    public required int PreviousThreshold { get; init; }
    public required int NextThreshold { get; init; }
    public required IReadOnlyList<HeartHistoryDto> History { get; init; }
}
