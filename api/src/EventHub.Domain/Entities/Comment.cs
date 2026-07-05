using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>Commentaire sur une publication (entité de l'agrégat Post).</summary>
public class Comment : BaseEntity
{
    public const string StatusPublished = "published";
    public const string StatusHidden = "hidden";

    private Comment() { } // EF Core

    public Guid PostId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Text { get; private set; } = null!;

    /// <summary>published | hidden | removed (modération).</summary>
    public string Status { get; private set; } = StatusPublished;

    public static Comment Create(Guid postId, Guid authorId, string text, DateTime nowUtc)
    {
        var comment = new Comment
        {
            PostId = Guard.AgainstEmpty(postId, nameof(postId)),
            AuthorId = Guard.AgainstEmpty(authorId, nameof(authorId)),
            Text = Guard.AgainstNullOrWhiteSpace(text, nameof(text)),
        };
        comment.MarkCreated(nowUtc);
        return comment;
    }

    /// <summary>Masque le commentaire (modération).</summary>
    public void Hide(DateTime nowUtc)
    {
        Status = StatusHidden;
        MarkUpdated(nowUtc);
    }
}
