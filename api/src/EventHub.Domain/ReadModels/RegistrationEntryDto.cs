namespace EventHub.Domain.ReadModels;

/// <summary>
/// Ligne de la liste des inscrits d'une activité pour le back office
/// (GET /api/admin/activities/{id}/registrations).
/// </summary>
public sealed record RegistrationEntryDto
{
    public required Guid UserId { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public required string Status { get; init; }
    public DateTime RegisteredAt { get; init; }
}
