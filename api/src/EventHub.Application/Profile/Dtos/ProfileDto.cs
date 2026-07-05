namespace EventHub.Application.Profile;

/// <summary>Profil de l'utilisateur courant (GET /api/me).</summary>
public sealed record ProfileDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? AvatarUrl { get; init; }
    public int TotalHearts { get; init; }
    public required string Level { get; init; }
    public int PreviousLevelThreshold { get; init; }
    public int NextLevelThreshold { get; init; }
    public IReadOnlyList<Guid> RegisteredActivityIds { get; init; } = [];
    public int RegistrationCount { get; init; }
}
