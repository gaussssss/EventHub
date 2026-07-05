namespace EventHub.Application.Registrations;

public enum CancellationResultStatus
{
    Cancelled,
    NotRegistered
}

public sealed record CancellationResult(
    CancellationResultStatus Status,
    Guid? PromotedUserId = null);
