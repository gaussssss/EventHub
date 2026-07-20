namespace EventHub.Domain.ReadModels;

/// <summary>Contributeur affiché sur la page « À propos » (mobile + back office).</summary>
public sealed record ContributorDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public string? AvatarUrl { get; init; }
    public int SortOrder { get; init; }
}
