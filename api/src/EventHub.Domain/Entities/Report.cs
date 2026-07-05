using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Signalement d'un contenu social (post ou commentaire) par un utilisateur.
/// Un modérateur le traite ensuite : <see cref="Resolve"/> (fondé) ou
/// <see cref="Dismiss"/> (rejeté).
/// </summary>
public class Report : BaseEntity
{
    private Report() { } // EF Core

    public Guid ReporterId { get; private set; }
    public ReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public string Reason { get; private set; } = null!;

    public ReportStatus Status { get; private set; } = ReportStatus.Open;

    public static Report Create(
        Guid reporterId, ReportTargetType targetType, Guid targetId, string reason, DateTime nowUtc)
    {
        var report = new Report
        {
            ReporterId = Guard.AgainstEmpty(reporterId, nameof(reporterId)),
            TargetType = targetType,
            TargetId = Guard.AgainstEmpty(targetId, nameof(targetId)),
            Reason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason)),
        };
        report.MarkCreated(nowUtc);
        return report;
    }

    /// <summary>Marque le signalement comme fondé (contenu modéré). Idempotent.</summary>
    public void Resolve() => Status = ReportStatus.Resolved;

    /// <summary>Rejette le signalement (contenu jugé conforme). Idempotent.</summary>
    public void Dismiss() => Status = ReportStatus.Dismissed;
}
