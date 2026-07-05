using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Supprimer sa propre publication (DELETE /api/posts/{id}).</summary>
public sealed record DeletePostCommand(Guid UserId, Guid PostId) : ICommand<DeletePostStatus>;
