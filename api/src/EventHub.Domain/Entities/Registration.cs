using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>Inscription d'un utilisateur à une activité (+ liste d'attente).</summary>
public class Registration : BaseEntity
{
    public const string SourceApp = "app";

    private Registration() { } // EF Core

    public Guid UserId { get; private set; }
    public Guid ActivityId { get; private set; }
    public Activity? Activity { get; private set; }

    public RegistrationStatus Status { get; private set; } = RegistrationStatus.Registered;

    /// <summary>"google_form" | "app" — origine de l'inscription.</summary>
    public string? Source { get; private set; }

    /// <summary>Identifiant de réponse Google Forms (réconciliation).</summary>
    public string? FormResponseId { get; private set; }

    public DateTime RegisteredAt { get; private set; } = DateTime.UtcNow;
    public DateTime? AttendedAt { get; private set; }

    /// <summary>Compte dans les places occupées (inscrit ou présent).</summary>
    public bool OccupiesSpot =>
        Status is RegistrationStatus.Registered or RegistrationStatus.Attended;

    public static Registration Create(
        Guid userId, Guid activityId, RegistrationStatus status,
        string source, string? formResponseId, DateTime nowUtc)
    {
        var registration = new Registration
        {
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            ActivityId = Guard.AgainstEmpty(activityId, nameof(activityId)),
            Status = status,
            Source = source,
            FormResponseId = formResponseId,
            RegisteredAt = nowUtc,
        };
        registration.MarkCreated(nowUtc);
        return registration;
    }

    /// <summary>
    /// (Ré)assigne le résultat d'une demande d'inscription — réactive une inscription
    /// annulée/absente ou réévalue le statut lors d'un rejeu de concurrence.
    /// </summary>
    public void AssignOutcome(
        RegistrationStatus status, string source, string? formResponseId, DateTime nowUtc)
    {
        Status = status;
        Source = source;
        FormResponseId = formResponseId;
        RegisteredAt = nowUtc;
        AttendedAt = null;
        MarkUpdated(nowUtc);
    }

    /// <summary>Annule l'inscription (libère éventuellement une place).</summary>
    public void Cancel(DateTime nowUtc)
    {
        Status = RegistrationStatus.Cancelled;
        MarkUpdated(nowUtc);
    }

    /// <summary>Promotion depuis la liste d'attente quand une place se libère.</summary>
    public void PromoteFromWaitlist(DateTime nowUtc)
    {
        if (Status != RegistrationStatus.Waitlisted)
            throw new DomainException("Seule une inscription en liste d'attente peut être promue.");

        Status = RegistrationStatus.Registered;
        RegisteredAt = nowUtc;
        MarkUpdated(nowUtc);
    }

    /// <summary>Marque la présence confirmée (crédit des cœurs côté cas d'usage).</summary>
    public void MarkAttended(DateTime nowUtc)
    {
        Status = RegistrationStatus.Attended;
        AttendedAt = nowUtc;
        MarkUpdated(nowUtc);
    }
}
