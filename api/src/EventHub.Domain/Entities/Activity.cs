using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Activité sportive ou socioculturelle (racine d'agrégat). Porte les règles
/// métier d'inscription (échéance, capacité, statut) — source de vérité serveur.
/// </summary>
public class Activity : BaseEntity
{
    private Activity() { } // EF Core

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public Guid? OrganizerId { get; private set; }
    public Organizer? Organizer { get; private set; }

    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }

    public string Location { get; private set; } = null!;
    public string ImageUrl { get; private set; } = null!;

    public int HeartsReward { get; private set; }
    public int MaxParticipants { get; private set; }

    public string? RegistrationUrl { get; private set; }
    public DateTime? RegistrationDeadline { get; private set; }

    public bool IsFeatured { get; private set; }
    public ActivityStatus Status { get; private set; } = ActivityStatus.Published;

    /// <summary>
    /// Jeton de concurrence optimiste. Change à chaque écriture : deux inscriptions
    /// concurrentes sur la dernière place entrent en conflit sur la même ligne
    /// <c>Activity</c> (une seule gagne, l'autre est rejouée puis mise en attente).
    /// </summary>
    public Guid Version { get; private set; } = Guid.NewGuid();

    public bool IsPublished => Status == ActivityStatus.Published;

    /// <summary>
    /// Réserve une place : marque l'agrégat modifié (nouveau <see cref="Version"/>)
    /// pour que la prise de la dernière place soit sérialisée entre requêtes.
    /// </summary>
    public void ClaimSpot(DateTime nowUtc)
    {
        Version = Guid.NewGuid();
        MarkUpdated(nowUtc);
    }

    public static Activity Create(
        string title, string description, Guid categoryId, Guid? organizerId,
        DateTime startsAt, DateTime? endsAt, string location, string imageUrl,
        int heartsReward, int maxParticipants, string? registrationUrl,
        DateTime? registrationDeadline, bool isFeatured, ActivityStatus status, DateTime nowUtc)
    {
        var activity = new Activity();
        activity.Apply(title, description, categoryId, organizerId, startsAt, endsAt,
            location, imageUrl, heartsReward, maxParticipants, registrationUrl,
            registrationDeadline, isFeatured, status);
        activity.MarkCreated(nowUtc);
        return activity;
    }

    /// <summary>Publie l'activité (draft/archived → published).</summary>
    public void Publish(DateTime nowUtc)
    {
        Status = ActivityStatus.Published;
        Version = Guid.NewGuid();
        MarkUpdated(nowUtc);
    }

    /// <summary>Annule l'activité (sort du catalogue public).</summary>
    public void Cancel(DateTime nowUtc)
    {
        Status = ActivityStatus.Cancelled;
        Version = Guid.NewGuid();
        MarkUpdated(nowUtc);
    }

    /// <summary>Bascule la mise « à la une ». Renvoie le nouvel état.</summary>
    public bool ToggleFeatured(DateTime nowUtc)
    {
        IsFeatured = !IsFeatured;
        Version = Guid.NewGuid();
        MarkUpdated(nowUtc);
        return IsFeatured;
    }

    /// <summary>Met à jour l'ensemble des attributs modifiables (back office).</summary>
    public void Update(
        string title, string description, Guid categoryId, Guid? organizerId,
        DateTime startsAt, DateTime? endsAt, string location, string imageUrl,
        int heartsReward, int maxParticipants, string? registrationUrl,
        DateTime? registrationDeadline, bool isFeatured, ActivityStatus status, DateTime nowUtc)
    {
        Apply(title, description, categoryId, organizerId, startsAt, endsAt,
            location, imageUrl, heartsReward, maxParticipants, registrationUrl,
            registrationDeadline, isFeatured, status);
        MarkUpdated(nowUtc);
    }

    private void Apply(
        string title, string description, Guid categoryId, Guid? organizerId,
        DateTime startsAt, DateTime? endsAt, string location, string imageUrl,
        int heartsReward, int maxParticipants, string? registrationUrl,
        DateTime? registrationDeadline, bool isFeatured, ActivityStatus status)
    {
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title));
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description));
        CategoryId = Guard.AgainstEmpty(categoryId, nameof(categoryId));
        OrganizerId = organizerId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Location = Guard.AgainstNullOrWhiteSpace(location, nameof(location));
        ImageUrl = Guard.AgainstNullOrWhiteSpace(imageUrl, nameof(imageUrl));
        HeartsReward = Guard.AgainstNegative(heartsReward, nameof(heartsReward));
        MaxParticipants = Guard.AgainstNonPositive(maxParticipants, nameof(maxParticipants));
        RegistrationUrl = registrationUrl;
        RegistrationDeadline = registrationDeadline;
        IsFeatured = isFeatured;
        Status = status;
        Version = Guid.NewGuid();
    }

    /// <summary>Vrai si l'activité est publiée et l'échéance non dépassée.</summary>
    public bool IsRegistrationOpen(DateTime nowUtc) =>
        IsPublished && (RegistrationDeadline is null || RegistrationDeadline > nowUtc);

    public bool HasCapacity(int currentParticipants) =>
        currentParticipants < MaxParticipants;

    /// <summary>
    /// Évalue une demande d'inscription : rejet si non publiée ou échéance
    /// passée, sinon inscrit s'il reste de la place, liste d'attente sinon.
    /// </summary>
    public RegistrationOutcome DetermineOutcome(int currentParticipants, DateTime nowUtc)
    {
        if (!IsPublished)
            return RegistrationOutcome.Rejected(RegistrationRejectionReason.NotPublished);

        if (!IsRegistrationOpen(nowUtc))
            return RegistrationOutcome.Rejected(RegistrationRejectionReason.DeadlinePassed);

        return HasCapacity(currentParticipants)
            ? RegistrationOutcome.Accepted(RegistrationStatus.Registered)
            : RegistrationOutcome.Accepted(RegistrationStatus.Waitlisted);
    }
}
