using EventHub.Application.Common.Messaging;
using EventHub.Domain.Enums;

namespace EventHub.Application.Moderation;

/// <summary>Commande « signaler un post ou un commentaire ».</summary>
public sealed record ReportContentCommand(
    Guid ReporterId, ReportTargetType TargetType, Guid TargetId, string Reason)
    : ICommand<ReportContentResult>;
