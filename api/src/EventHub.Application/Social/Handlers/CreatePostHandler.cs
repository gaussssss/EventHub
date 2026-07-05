using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Entities;

namespace EventHub.Application.Social;

public sealed class CreatePostHandler : ICommandHandler<CreatePostCommand, Guid>
{
    private readonly IPostRepository _posts;
    private readonly IClock _clock;

    public CreatePostHandler(IPostRepository posts, IClock clock)
    {
        _posts = posts;
        _clock = clock;
    }

    public async Task<Guid> HandleAsync(
        CreatePostCommand command, CancellationToken cancellationToken = default)
    {
        var post = Post.Create(
            command.AuthorId, command.ImageUrl, command.Caption, command.ActivityId, _clock.UtcNow);

        await _posts.AddAsync(post, cancellationToken);
        await _posts.SaveChangesAsync(cancellationToken);
        return post.Id;
    }
}
