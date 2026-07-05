using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Moderation;

/// <summary>Commande « masquer un commentaire » (modérateur).</summary>
public sealed record HideCommentCommand(Guid CommentId) : ICommand<HideResult>;
