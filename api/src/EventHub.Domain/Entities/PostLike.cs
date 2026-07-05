using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>« J'aime » d'un utilisateur sur une publication (clé composite Post+User).</summary>
public class PostLike
{
    private PostLike() { } // EF Core

    public Guid PostId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public static PostLike Create(Guid postId, Guid userId, DateTime nowUtc)
    {
        return new PostLike
        {
            PostId = Guard.AgainstEmpty(postId, nameof(postId)),
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            CreatedAt = nowUtc,
        };
    }
}
