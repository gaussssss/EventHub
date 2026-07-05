using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Enums;
using EventHub.Domain.Repositories;

namespace EventHub.Application.Moderation;

/// <summary>Masque un commentaire (voir <see cref="HidePostHandler"/>).</summary>
public sealed class HideCommentHandler : ICommandHandler<HideCommentCommand, HideResult>
{
    private readonly IPostRepository _posts;
    private readonly IReportRepository _reports;
    private readonly IClock _clock;

    public HideCommentHandler(IPostRepository posts, IReportRepository reports, IClock clock)
    {
        _posts = posts;
        _reports = reports;
        _clock = clock;
    }

    public async Task<HideResult> HandleAsync(
        HideCommentCommand command, CancellationToken cancellationToken = default)
    {
        var comment = await _posts.GetCommentAsync(command.CommentId, cancellationToken);
        if (comment is null)
            return HideResult.NotFound;

        comment.Hide(_clock.UtcNow);
        await ResolveReports.ForTargetAsync(
            _reports, ReportTargetType.Comment, command.CommentId, cancellationToken);
        await _posts.SaveChangesAsync(cancellationToken);
        return HideResult.Hidden;
    }
}
