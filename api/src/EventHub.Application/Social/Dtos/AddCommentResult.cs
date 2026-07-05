namespace EventHub.Application.Social;

public enum AddCommentStatus
{
    Added,
    PostNotFound
}

public sealed record AddCommentResult(AddCommentStatus Status, Guid? CommentId = null);
