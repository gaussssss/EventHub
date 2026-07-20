namespace EventHub.Domain.ReadModels;

/// <summary>
/// Contrat JSON figé par l'app mobile (voir docs/BACKEND_MANIFEST.md §3.0).
/// Sérialisé en camelCase : category = slug, organizer = chaîne, cœurs sous
/// heartsReward, dates ISO-8601 UTC.
/// </summary>
public sealed record ActivityDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public string? Organizer { get; init; }
    public required DateTime StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public required string Location { get; init; }
    public required string ImageUrl { get; init; }
    public int HeartsReward { get; init; }
    public int MaxParticipants { get; init; }
    public int CurrentParticipants { get; init; }
    public decimal ParticipationCost { get; init; }
    public string? RegistrationUrl { get; init; }
    public DateTime? RegistrationDeadline { get; init; }
    public bool IsFeatured { get; init; }

    /// <summary>
    /// Statut d'inscription de l'utilisateur courant, **uniquement** renseigné par
    /// « mes inscriptions » (GET /api/me/registrations) : <c>registered</c> |
    /// <c>attended</c> | <c>noshow</c> | <c>waitlisted</c>. <c>null</c> ailleurs.
    /// </summary>
    public string? MyStatus { get; init; }
}
