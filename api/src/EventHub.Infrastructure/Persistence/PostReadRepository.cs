using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using System.Linq.Expressions;
using EventHub.Application.Social;
using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

/// <summary>
/// Projection EF Core du fil communautaire : joint l'auteur (Identity) et
/// l'activité liée, compte les « j'aime » et embarque les commentaires publiés.
/// </summary>
public sealed class PostReadRepository : IPostReadRepository
{
    private readonly EventHubDbContext _db;

    public PostReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<PostDto>> GetFeedAsync(CancellationToken cancellationToken = default) =>
        await _db.Posts.AsNoTracking()
            .Where(p => p.Status == "published")
            .OrderByDescending(p => p.CreatedAt)
            .Select(ToDto())
            .ToListAsync(cancellationToken);

    public async Task<PostDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Posts.AsNoTracking()
            .Where(p => p.Id == id && p.Status == "published")
            .Select(ToDto())
            .FirstOrDefaultAsync(cancellationToken);

    private Expression<Func<Post, PostDto>> ToDto() => p => new PostDto
    {
        Id = p.Id,
        AuthorName = _db.Users.Where(u => u.Id == p.AuthorId).Select(u => u.Name).FirstOrDefault()
                     ?? string.Empty,
        AuthorAvatarUrl = _db.Users.Where(u => u.Id == p.AuthorId).Select(u => u.AvatarUrl).FirstOrDefault(),
        ImageUrl = p.ImageUrl,
        Caption = p.Caption,
        ActivityName = p.ActivityId == null
            ? null
            : _db.Activities.Where(a => a.Id == p.ActivityId).Select(a => a.Title).FirstOrDefault(),
        CreatedAt = p.CreatedAt,
        LikesCount = p.Likes.Count,
        Comments = p.Comments
            .Where(c => c.Status == "published")
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(
                _db.Users.Where(u => u.Id == c.AuthorId).Select(u => u.Name).FirstOrDefault()
                    ?? string.Empty,
                c.Text,
                c.CreatedAt))
            .ToList(),
    };
}
