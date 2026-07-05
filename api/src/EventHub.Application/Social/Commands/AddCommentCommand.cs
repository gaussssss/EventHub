using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Commande « commenter une publication ».</summary>
public sealed record AddCommentCommand(Guid AuthorId, Guid PostId, string Text)
    : ICommand<AddCommentResult>;
