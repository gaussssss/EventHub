namespace EventHub.Domain.ReadModels;

/// <summary>
/// Ligne de la liste d'activités au back office (GET /api/admin/activities).
/// Contrairement à <see cref="ActivityDto"/> (contrat mobile, publié uniquement),
/// inclut le <c>Status</c> et couvre tous les statuts (brouillons, annulées…).
/// </summary>
public sealed record AdminActivityDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Category { get; init; }
    public required DateTime StartsAt { get; init; }
    public required string Location { get; init; }
    public required string Status { get; init; }
    public bool IsFeatured { get; init; }
    public int MaxParticipants { get; init; }
    public int CurrentParticipants { get; init; }
}
