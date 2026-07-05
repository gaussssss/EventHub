using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Moderation;

/// <summary>Commande « masquer une publication » (modérateur).</summary>
public sealed record HidePostCommand(Guid PostId) : ICommand<HideResult>;
