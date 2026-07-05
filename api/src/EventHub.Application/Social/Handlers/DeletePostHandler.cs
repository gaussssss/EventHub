using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>
/// Suppression d'une publication par son auteur (masquage = retrait du fil).
/// La modération dispose de son propre endpoint (POST /api/admin/posts/{id}/hide).
/// </summary>
public sealed class DeletePostHandler : ICommandHandler<DeletePostCommand, DeletePostStatus>
{
    private readonly IPostRepository _posts;
    private readonly IClock _clock;

    public DeletePostHandler(IPostRepository posts, IClock clock)
    {
        _posts = posts;
        _clock = clock;
    }

    public async Task<DeletePostStatus> HandleAsync(
        DeletePostCommand command, CancellationToken cancellationToken = default)
    {
        var post = await _posts.GetPostAsync(command.PostId, cancellationToken);
        if (post is null)
            return DeletePostStatus.NotFound;

        if (post.AuthorId != command.UserId)
            return DeletePostStatus.Forbidden;

        post.Hide(_clock.UtcNow);
        await _posts.SaveChangesAsync(cancellationToken);
        return DeletePostStatus.Deleted;
    }
}
