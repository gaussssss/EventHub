namespace EventHub.Application.Moderation;

public enum ReportContentStatus
{
    Created,
    TargetNotFound
}

public sealed record ReportContentResult(ReportContentStatus Status, Guid? ReportId);
