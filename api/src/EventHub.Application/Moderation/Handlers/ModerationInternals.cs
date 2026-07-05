using EventHub.Domain.Enums;
using EventHub.Domain.Repositories;

namespace EventHub.Application.Moderation;

internal static class ResolveReports
{
    /// <summary>Clôt (Resolve) les signalements ouverts visant la cible modérée.</summary>
    public static async Task ForTargetAsync(
        IReportRepository reports, ReportTargetType targetType, Guid targetId,
        CancellationToken cancellationToken)
    {
        var open = await reports.GetOpenForTargetAsync(targetType, targetId, cancellationToken);
        foreach (var report in open)
            report.Resolve();
    }
}
