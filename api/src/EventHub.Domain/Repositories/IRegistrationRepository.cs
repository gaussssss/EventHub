using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

public interface IRegistrationRepository
{
    /// <summary>Nombre d'inscriptions occupant une place (Registered/Attended).</summary>
    Task<int> CountActiveAsync(Guid activityId, CancellationToken cancellationToken = default);

    Task<Registration?> FindAsync(
        Guid userId, Guid activityId, CancellationToken cancellationToken = default);

    /// <summary>Première personne en liste d'attente (par ordre d'arrivée), pour promotion.</summary>
    Task<Registration?> FindFirstWaitlistedAsync(
        Guid activityId, CancellationToken cancellationToken = default);

    /// <summary>Inscriptions d'une activité pour un ensemble d'utilisateurs (prise de présence).</summary>
    Task<IReadOnlyList<Registration>> GetForActivityAsync(
        Guid activityId, IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    /// <summary>Ids des activités où l'utilisateur occupe une place (Registered/Attended).</summary>
    Task<IReadOnlyList<Guid>> GetActiveActivityIdsAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Registration registration, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
