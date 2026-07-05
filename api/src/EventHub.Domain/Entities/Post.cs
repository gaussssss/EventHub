using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Publication du fil communautaire (racine d'agrégat : photo + légende, liée à
/// une activité, avec ses commentaires et « j'aime »).
/// </summary>
public class Post : BaseEntity
{
    public const string StatusPublished = "published";
    public const string StatusHidden = "hidden";

    private readonly List<Comment> _comments = new();
    private readonly List<PostLike> _likes = new();

    private Post() { } // EF Core

    public Guid AuthorId { get; private set; }
    public Guid? ActivityId { get; private set; }

    public string ImageUrl { get; private set; } = null!;
    public string Caption { get; private set; } = null!;

    /// <summary>published | hidden | removed (modération).</summary>
    public string Status { get; private set; } = StatusPublished;

    public IReadOnlyCollection<Comment> Comments => _comments;
    public IReadOnlyCollection<PostLike> Likes => _likes;

    public static Post Create(
        Guid authorId, string imageUrl, string caption, Guid? activityId, DateTime nowUtc)
    {
        var post = new Post
        {
            AuthorId = Guard.AgainstEmpty(authorId, nameof(authorId)),
            ImageUrl = Guard.AgainstNullOrWhiteSpace(imageUrl, nameof(imageUrl)),
            Caption = Guard.AgainstNullOrWhiteSpace(caption, nameof(caption)),
            ActivityId = activityId,
        };
        post.MarkCreated(nowUtc);
        return post;
    }

    /// <summary>Masque la publication (modération) — disparaît des projections publiques.</summary>
    public void Hide(DateTime nowUtc)
    {
        Status = StatusHidden;
        MarkUpdated(nowUtc);
    }
}
