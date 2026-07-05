using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Commande « aimer une publication » (idempotente).</summary>
public sealed record LikePostCommand(Guid UserId, Guid PostId) : ICommand<LikeResult>;
