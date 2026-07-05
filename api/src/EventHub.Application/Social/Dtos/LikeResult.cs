namespace EventHub.Application.Social;

public enum LikeResultStatus
{
    Updated,
    PostNotFound
}

public sealed record LikeResult(LikeResultStatus Status, int LikesCount);
