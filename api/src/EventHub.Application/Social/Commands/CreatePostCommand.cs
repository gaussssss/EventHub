using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Commande « publier une photo » ; renvoie l'identifiant du post créé.</summary>
public sealed record CreatePostCommand(
    Guid AuthorId, string ImageUrl, string Caption, Guid? ActivityId = null)
    : ICommand<Guid>;
