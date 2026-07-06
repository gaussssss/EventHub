using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

/// <summary>Requête « fil communautaire ».</summary>
public sealed record GetFeedQuery(Guid? CurrentUserId = null)
    : IQuery<IReadOnlyList<PostDto>>;
