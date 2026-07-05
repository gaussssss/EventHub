namespace EventHub.Domain.ReadModels;

/// <summary>Chip de catégorie exposée à l'app (GET /api/categories).</summary>
public sealed record CategoryDto
{
    public required Guid Id { get; init; }
    public required string Slug { get; init; }
    public required string Label { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
}
