using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

public sealed class GetFeedHandler : IQueryHandler<GetFeedQuery, IReadOnlyList<PostDto>>
{
    private readonly IPostReadRepository _posts;

    public GetFeedHandler(IPostReadRepository posts) => _posts = posts;

    public Task<IReadOnlyList<PostDto>> HandleAsync(
        GetFeedQuery query, CancellationToken cancellationToken = default) =>
        _posts.GetFeedAsync(cancellationToken);
}
