namespace EventHub.Domain.ReadModels;

/// <summary>Organisateur exposé au back office (GET /api/admin/organizers).</summary>
public sealed record OrganizerDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? ContactEmail { get; init; }
}
