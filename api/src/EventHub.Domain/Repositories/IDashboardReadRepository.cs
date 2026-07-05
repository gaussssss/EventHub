using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

/// <summary>Agrégats du tableau de bord et données d'export (back office).</summary>
public interface IDashboardReadRepository
{
    Task<DashboardOverviewDto> GetOverviewAsync(
        DateTime nowUtc, CancellationToken cancellationToken = default);

    /// <summary>Statistiques d'une activité (remplissage, présence, no-show), ou null.</summary>
    Task<ActivityDashboardDto?> GetActivityDashboardAsync(
        Guid activityId, CancellationToken cancellationToken = default);

    /// <summary>Toutes les inscriptions à plat (activité + utilisateur) pour l'export CSV.</summary>
    Task<IReadOnlyList<RegistrationExportRow>> GetRegistrationsForExportAsync(
        CancellationToken cancellationToken = default);
}
