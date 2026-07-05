using EventHub.Domain.Repositories;
using EventHub.Application.Social;
using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class PostRepository : IPostRepository
{
    private readonly EventHubDbContext _db;

    public PostRepository(EventHubDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid postId, CancellationToken cancellationToken = default) =>
        _db.Posts.AnyAsync(p => p.Id == postId && p.Status == "published", cancellationToken);

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default) =>
        await _db.Posts.AddAsync(post, cancellationToken);

    public Task<bool> LikeExistsAsync(
        Guid postId, Guid userId, CancellationToken cancellationToken = default) =>
        _db.PostLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId, cancellationToken);

    public async Task AddLikeAsync(PostLike like, CancellationToken cancellationToken = default) =>
        await _db.PostLikes.AddAsync(like, cancellationToken);

    public async Task RemoveLikeAsync(
        Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        var like = await _db.PostLikes.FirstOrDefaultAsync(
            l => l.PostId == postId && l.UserId == userId, cancellationToken);
        if (like is not null)
            _db.PostLikes.Remove(like);
    }

    public Task<int> CountLikesAsync(Guid postId, CancellationToken cancellationToken = default) =>
        _db.PostLikes.CountAsync(l => l.PostId == postId, cancellationToken);

    public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default) =>
        await _db.Comments.AddAsync(comment, cancellationToken);

    public Task<bool> CommentExistsAsync(Guid commentId, CancellationToken cancellationToken = default) =>
        _db.Comments.AnyAsync(c => c.Id == commentId && c.Status == "published", cancellationToken);

    public Task<Post?> GetPostAsync(Guid postId, CancellationToken cancellationToken = default) =>
        _db.Posts.FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

    public Task<Comment?> GetCommentAsync(Guid commentId, CancellationToken cancellationToken = default) =>
        _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
