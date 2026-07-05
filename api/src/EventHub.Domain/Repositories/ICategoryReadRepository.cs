using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

/// <summary>Accès en lecture au référentiel des catégories (GET /api/categories).</summary>
public interface ICategoryReadRepository
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
