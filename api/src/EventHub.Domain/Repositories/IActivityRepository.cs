using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

/// <summary>Accès en écriture/lecture aux activités (côté commande).</summary>
public interface IActivityRepository
{
    Task<Activity?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Activity activity, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
