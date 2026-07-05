using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Enums;
using EventHub.Domain.Repositories;

namespace EventHub.Application.Moderation;

/// <summary>
/// Masque une publication : la racine d'agrégat passe en <c>hidden</c> (disparaît
/// des projections publiques) et les signalements ouverts qui la visent sont clôturés.
/// </summary>
public sealed class HidePostHandler : ICommandHandler<HidePostCommand, HideResult>
{
    private readonly IPostRepository _posts;
    private readonly IReportRepository _reports;
    private readonly IClock _clock;

    public HidePostHandler(IPostRepository posts, IReportRepository reports, IClock clock)
    {
        _posts = posts;
        _reports = reports;
        _clock = clock;
    }

    public async Task<HideResult> HandleAsync(
        HidePostCommand command, CancellationToken cancellationToken = default)
    {
        var post = await _posts.GetPostAsync(command.PostId, cancellationToken);
        if (post is null)
            return HideResult.NotFound;

        post.Hide(_clock.UtcNow);
        await ResolveReports.ForTargetAsync(
            _reports, ReportTargetType.Post, command.PostId, cancellationToken);
        await _posts.SaveChangesAsync(cancellationToken);
        return HideResult.Hidden;
    }
}
