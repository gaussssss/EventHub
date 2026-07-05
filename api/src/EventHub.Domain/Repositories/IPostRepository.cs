using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

/// <summary>Écritures du fil communautaire (posts, likes, commentaires).</summary>
public interface IPostRepository
{
    Task<bool> ExistsAsync(Guid postId, CancellationToken cancellationToken = default);

    Task AddAsync(Post post, CancellationToken cancellationToken = default);

    Task<bool> LikeExistsAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task AddLikeAsync(PostLike like, CancellationToken cancellationToken = default);
    Task RemoveLikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountLikesAsync(Guid postId, CancellationToken cancellationToken = default);

    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<bool> CommentExistsAsync(Guid commentId, CancellationToken cancellationToken = default);

    // Modération : chargement suivi (tracked), quel que soit le statut.
    Task<Post?> GetPostAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<Comment?> GetCommentAsync(Guid commentId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
