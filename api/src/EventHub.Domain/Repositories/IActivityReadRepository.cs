using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

/// <summary>
/// Accès en lecture au catalogue d'activités (côté requête). L'implémentation
/// (Infrastructure) projette les entités + le nombre d'inscrits en [ActivityDto].
/// </summary>
public interface IActivityReadRepository
{
    Task<IReadOnlyList<ActivityDto>> GetActivitiesAsync(
        ActivityFilter filter,
        CancellationToken cancellationToken = default);

    Task<ActivityDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toutes les activités (tous statuts confondus) pour le back office,
    /// triées de la plus récente à la plus ancienne. Inclut le statut.
    /// </summary>
    Task<IReadOnlyList<AdminActivityDto>> GetAllForAdminAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Détail complet d'une activité pour l'édition (back office), ou null.</summary>
    Task<AdminActivityDetailDto?> GetForAdminByIdAsync(
        Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activités auxquelles l'utilisateur est inscrit (statuts non annulés) —
    /// page Profil « Mes activités ».
    /// </summary>
    Task<IReadOnlyList<ActivityDto>> GetRegisteredByUserAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
