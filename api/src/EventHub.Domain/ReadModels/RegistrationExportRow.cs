namespace EventHub.Domain.ReadModels;

/// <summary>Ligne à plat pour l'export CSV des inscriptions.</summary>
public sealed record RegistrationExportRow
{
    public required Guid ActivityId { get; init; }
    public required string ActivityTitle { get; init; }
    public required Guid UserId { get; init; }
    public string? UserName { get; init; }
    public string? UserEmail { get; init; }
    public required string Status { get; init; }
    public DateTime RegisteredAt { get; init; }
}
