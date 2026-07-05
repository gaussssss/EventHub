using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Requête « fil communautaire ».</summary>
public sealed record GetFeedQuery : IQuery<IReadOnlyList<PostDto>>;
