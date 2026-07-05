using EventHub.Domain.Entities;

namespace EventHub.Application.Registrations;

public enum RegistrationResultStatus
{
    Registered,
    Waitlisted,
    AlreadyRegistered,
    Rejected,
    ActivityNotFound
}

public sealed record RegistrationResult(
    RegistrationResultStatus Status,
    RegistrationRejectionReason? Reason = null);
