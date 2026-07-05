using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Entities;

namespace EventHub.Application.Social;

/// <summary>Ajoute un « j'aime » (idempotent) et renvoie le nouveau total.</summary>
public sealed class LikePostHandler : ICommandHandler<LikePostCommand, LikeResult>
{
    private readonly IPostRepository _posts;
    private readonly IClock _clock;

    public LikePostHandler(IPostRepository posts, IClock clock)
    {
        _posts = posts;
        _clock = clock;
    }

    public async Task<LikeResult> HandleAsync(
        LikePostCommand command, CancellationToken cancellationToken = default)
    {
        var (userId, postId) = command;
        if (!await _posts.ExistsAsync(postId, cancellationToken))
            return new LikeResult(LikeResultStatus.PostNotFound, 0);

        if (!await _posts.LikeExistsAsync(postId, userId, cancellationToken))
        {
            await _posts.AddLikeAsync(
                PostLike.Create(postId, userId, _clock.UtcNow), cancellationToken);
            await _posts.SaveChangesAsync(cancellationToken);
        }

        var count = await _posts.CountLikesAsync(postId, cancellationToken);
        return new LikeResult(LikeResultStatus.Updated, count);
    }
}
