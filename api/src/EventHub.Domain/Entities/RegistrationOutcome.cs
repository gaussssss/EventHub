using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public enum RegistrationRejectionReason
{
    NotPublished,
    DeadlinePassed
}

/// <summary>
/// Résultat de l'évaluation d'une demande d'inscription à une activité :
/// acceptée (inscrit ou liste d'attente) ou rejetée (avec motif).
/// </summary>
public sealed class RegistrationOutcome
{
    public bool IsAccepted { get; }
    public RegistrationStatus? Status { get; }
    public RegistrationRejectionReason? Reason { get; }

    private RegistrationOutcome(
        bool isAccepted,
        RegistrationStatus? status,
        RegistrationRejectionReason? reason)
    {
        IsAccepted = isAccepted;
        Status = status;
        Reason = reason;
    }

    public static RegistrationOutcome Accepted(RegistrationStatus status) =>
        new(true, status, null);

    public static RegistrationOutcome Rejected(RegistrationRejectionReason reason) =>
        new(false, null, reason);
}
