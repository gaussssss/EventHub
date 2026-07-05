namespace EventHub.Domain.Common;

/// <summary>
/// Base de toutes les entités persistées (identité + horodatage). Les setters
/// sont protégés : l'état ne se modifie qu'via les fabriques et méthodes métier
/// des agrégats.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>Fixe les horodatages à la création (appelé par les fabriques).</summary>
    protected void MarkCreated(DateTime nowUtc)
    {
        CreatedAt = nowUtc;
        UpdatedAt = nowUtc;
    }

    /// <summary>Avance l'horodatage de modification (appelé par les méthodes métier).</summary>
    protected void MarkUpdated(DateTime nowUtc) => UpdatedAt = nowUtc;
}
