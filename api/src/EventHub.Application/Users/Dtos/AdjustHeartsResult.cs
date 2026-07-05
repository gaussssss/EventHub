namespace EventHub.Application.Users;

public enum AdjustHeartsStatus
{
    Adjusted,
    UserNotFound,
    InvalidAmount
}

public sealed record AdjustHeartsResult(AdjustHeartsStatus Status, int NewTotal);
