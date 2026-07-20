using EventHub.Domain.ReadModels;

namespace EventHub.Domain.Repositories;

/// <summary>Lecture des contributeurs (page « À propos »).</summary>
public interface IContributorReadRepository
{
    Task<IReadOnlyList<ContributorDto>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
