using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Retire un « j'aime » (idempotent) et renvoie le nouveau total.</summary>
public sealed class UnlikePostHandler : ICommandHandler<UnlikePostCommand, LikeResult>
{
    private readonly IPostRepository _posts;

    public UnlikePostHandler(IPostRepository posts) => _posts = posts;

    public async Task<LikeResult> HandleAsync(
        UnlikePostCommand command, CancellationToken cancellationToken = default)
    {
        var (userId, postId) = command;
        if (!await _posts.ExistsAsync(postId, cancellationToken))
            return new LikeResult(LikeResultStatus.PostNotFound, 0);

        if (await _posts.LikeExistsAsync(postId, userId, cancellationToken))
        {
            await _posts.RemoveLikeAsync(postId, userId, cancellationToken);
            await _posts.SaveChangesAsync(cancellationToken);
        }

        var count = await _posts.CountLikesAsync(postId, cancellationToken);
        return new LikeResult(LikeResultStatus.Updated, count);
    }
}
