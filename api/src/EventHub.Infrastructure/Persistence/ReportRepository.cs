using EventHub.Domain.Repositories;
using EventHub.Application.Moderation;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class ReportRepository : IReportRepository
{
    private readonly EventHubDbContext _db;

    public ReportRepository(EventHubDbContext db) => _db = db;

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default) =>
        await _db.Reports.AddAsync(report, cancellationToken);

    public async Task<IReadOnlyList<Report>> GetOpenForTargetAsync(
        ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default) =>
        await _db.Reports
            .Where(r => r.TargetType == targetType
                        && r.TargetId == targetId
                        && r.Status == ReportStatus.Open)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
