using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Commande « retirer son j'aime » (idempotente).</summary>
public sealed record UnlikePostCommand(Guid UserId, Guid PostId) : ICommand<LikeResult>;
