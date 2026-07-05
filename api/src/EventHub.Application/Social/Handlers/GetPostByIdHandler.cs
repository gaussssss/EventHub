using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Social;

public sealed class GetPostByIdHandler : IQueryHandler<GetPostByIdQuery, PostDto?>
{
    private readonly IPostReadRepository _posts;

    public GetPostByIdHandler(IPostReadRepository posts) => _posts = posts;

    public Task<PostDto?> HandleAsync(
        GetPostByIdQuery query, CancellationToken cancellationToken = default) =>
        _posts.GetByIdAsync(query.Id, cancellationToken);
}
