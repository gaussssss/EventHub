using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

/// <summary>Écritures sur le référentiel des contributeurs (back office).</summary>
public interface IContributorRepository
{
    Task<Contributor?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Contributor contributor, CancellationToken cancellationToken = default);
    void Remove(Contributor contributor);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
