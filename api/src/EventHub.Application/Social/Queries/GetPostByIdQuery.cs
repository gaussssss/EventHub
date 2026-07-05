using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Requête « détail d'une publication ».</summary>
public sealed record GetPostByIdQuery(Guid Id) : IQuery<PostDto?>;
