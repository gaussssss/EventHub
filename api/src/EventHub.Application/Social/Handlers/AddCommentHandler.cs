using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Entities;

namespace EventHub.Application.Social;

public sealed class AddCommentHandler : ICommandHandler<AddCommentCommand, AddCommentResult>
{
    private readonly IPostRepository _posts;
    private readonly IClock _clock;

    public AddCommentHandler(IPostRepository posts, IClock clock)
    {
        _posts = posts;
        _clock = clock;
    }

    public async Task<AddCommentResult> HandleAsync(
        AddCommentCommand command, CancellationToken cancellationToken = default)
    {
        if (!await _posts.ExistsAsync(command.PostId, cancellationToken))
            return new AddCommentResult(AddCommentStatus.PostNotFound);

        var comment = Comment.Create(
            command.PostId, command.AuthorId, command.Text, _clock.UtcNow);

        await _posts.AddCommentAsync(comment, cancellationToken);
        await _posts.SaveChangesAsync(cancellationToken);
        return new AddCommentResult(AddCommentStatus.Added, comment.Id);
    }
}
