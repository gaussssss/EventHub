namespace EventHub.Application.Admin;

public enum CreateActivityStatus
{
    Created,
    CategoryNotFound
}

public sealed record CreateActivityResult(CreateActivityStatus Status, Guid? ActivityId = null);
