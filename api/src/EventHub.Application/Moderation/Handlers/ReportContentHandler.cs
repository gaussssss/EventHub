using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Social;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;

namespace EventHub.Application.Moderation;

/// <summary>Signalement d'un post ou d'un commentaire par un utilisateur (POST /api/reports).</summary>
public sealed class ReportContentHandler
    : ICommandHandler<ReportContentCommand, ReportContentResult>
{
    private readonly IReportRepository _reports;
    private readonly IPostRepository _posts;
    private readonly IClock _clock;

    public ReportContentHandler(IReportRepository reports, IPostRepository posts, IClock clock)
    {
        _reports = reports;
        _posts = posts;
        _clock = clock;
    }

    public async Task<ReportContentResult> HandleAsync(
        ReportContentCommand command, CancellationToken cancellationToken = default)
    {
        var exists = command.TargetType == ReportTargetType.Post
            ? await _posts.ExistsAsync(command.TargetId, cancellationToken)
            : await _posts.CommentExistsAsync(command.TargetId, cancellationToken);

        if (!exists)
            return new ReportContentResult(ReportContentStatus.TargetNotFound, null);

        var report = Report.Create(
            command.ReporterId, command.TargetType, command.TargetId, command.Reason, _clock.UtcNow);

        await _reports.AddAsync(report, cancellationToken);
        await _reports.SaveChangesAsync(cancellationToken);

        return new ReportContentResult(ReportContentStatus.Created, report.Id);
    }
}
