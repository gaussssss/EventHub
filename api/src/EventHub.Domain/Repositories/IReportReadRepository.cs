using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

/// <summary>Lecture de la file de signalements (projection).</summary>
public interface IReportReadRepository
{
    Task<IReadOnlyList<ReportDto>> GetOpenAsync(CancellationToken cancellationToken = default);
}
