namespace EventHub.Domain.ReadModels;

/// <summary>Notification in-app exposée à l'app (GET /api/me/notifications).</summary>
public sealed record NotificationDto
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public string? Data { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
