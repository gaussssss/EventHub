namespace EventHub.Domain.ReadModels;

/// <summary>
/// Détail complet d'une activité pour l'édition au back office
/// (GET /api/admin/activities/{id}). Inclut les identifiants bruts
/// (CategoryId, OrganizerId) nécessaires au formulaire.
/// </summary>
public sealed record AdminActivityDetailDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required Guid CategoryId { get; init; }
    public Guid? OrganizerId { get; init; }
    public required DateTime StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public required string Location { get; init; }
    public required string ImageUrl { get; init; }
    public int HeartsReward { get; init; }
    public int MaxParticipants { get; init; }
    public decimal ParticipationCost { get; init; }
    public string? RegistrationUrl { get; init; }
    public DateTime? RegistrationDeadline { get; init; }
    public bool IsFeatured { get; init; }
    public required string Status { get; init; }

    /// <summary>Jeton d'émargement (admin uniquement) : encodé dans le QR de présence.</summary>
    public Guid CheckInToken { get; init; }
}
