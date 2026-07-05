using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Hearts;

/// <summary>Requête « résumé des cœurs » de l'utilisateur.</summary>
public sealed record GetHeartsSummaryQuery(Guid UserId) : IQuery<HeartsSummaryDto>;
