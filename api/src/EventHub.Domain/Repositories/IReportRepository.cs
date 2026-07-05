using EventHub.Domain.Entities;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Repositories;

/// <summary>Écritures de la file de signalements (modération).</summary>
public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);

    /// <summary>Signalements encore ouverts visant un contenu donné (pour clôture auto).</summary>
    Task<IReadOnlyList<Report>> GetOpenForTargetAsync(
        ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
