using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Moderation;
using EventHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

/// <summary>
/// Projection de la file des signalements ouverts, enrichie du contenu visé
/// (aperçu du texte/caption, image, auteur) pour que le modérateur voie ce
/// qui est signalé avant d'agir.
/// </summary>
public sealed class ReportReadRepository : IReportReadRepository
{
    private readonly EventHubDbContext _db;

    public ReportReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReportDto>> GetOpenAsync(CancellationToken cancellationToken = default)
    {
        var reports = await _db.Reports.AsNoTracking()
            .Where(r => r.Status == ReportStatus.Open)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.TargetType,
                r.TargetId,
                r.Reason,
                r.Status,
                r.ReporterId,
                r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        if (reports.Count == 0)
            return [];

        var postIds = reports.Where(r => r.TargetType == ReportTargetType.Post)
            .Select(r => r.TargetId).Distinct().ToList();
        var commentIds = reports.Where(r => r.TargetType == ReportTargetType.Comment)
            .Select(r => r.TargetId).Distinct().ToList();

        var posts = await _db.Posts.AsNoTracking()
            .Where(p => postIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Caption, p.ImageUrl, p.AuthorId })
            .ToListAsync(cancellationToken);
        var comments = await _db.Comments.AsNoTracking()
            .Where(c => commentIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Text, c.AuthorId })
            .ToListAsync(cancellationToken);

        var authorIds = posts.Select(p => p.AuthorId)
            .Concat(comments.Select(c => c.AuthorId))
            .Concat(reports.Select(r => r.ReporterId))
            .Distinct().ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(u => authorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

        var postMap = posts.ToDictionary(p => p.Id);
        var commentMap = comments.ToDictionary(c => c.Id);

        return reports.Select(r =>
        {
            string? preview = null, imageUrl = null, authorName = null;
            if (r.TargetType == ReportTargetType.Post && postMap.TryGetValue(r.TargetId, out var p))
            {
                preview = p.Caption;
                imageUrl = p.ImageUrl;
                authorName = names.GetValueOrDefault(p.AuthorId);
            }
            else if (r.TargetType == ReportTargetType.Comment && commentMap.TryGetValue(r.TargetId, out var c))
            {
                preview = c.Text;
                authorName = names.GetValueOrDefault(c.AuthorId);
            }

            return new ReportDto(
                r.Id,
                r.TargetType.ToString(),
                r.TargetId,
                r.Reason,
                r.Status.ToString(),
                names.GetValueOrDefault(r.ReporterId) ?? string.Empty,
                r.CreatedAt,
                preview,
                imageUrl,
                authorName);
        }).ToList();
    }
}
