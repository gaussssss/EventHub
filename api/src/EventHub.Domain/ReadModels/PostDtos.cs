namespace EventHub.Domain.ReadModels;

public sealed record CommentDto(Guid Id, string AuthorName, string Text, DateTime CreatedAt);

/// <summary>Publication du fil (contrat mobile : voir docs/BACKEND_MANIFEST.md §3.0).</summary>
public sealed record PostDto
{
    public required Guid Id { get; init; }
    public required string AuthorName { get; init; }
    public string? AuthorAvatarUrl { get; init; }
    public required string ImageUrl { get; init; }
    public required string Caption { get; init; }
    public string? ActivityName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public int LikesCount { get; init; }

    /// <summary>Vrai si l'utilisateur courant a « aimé » ce post (fil authentifié).</summary>
    public bool IsLikedByMe { get; init; }

    public IReadOnlyList<CommentDto> Comments { get; init; } = [];
}
