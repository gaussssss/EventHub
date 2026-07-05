using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

public interface IPostReadRepository
{
    Task<IReadOnlyList<PostDto>> GetFeedAsync(CancellationToken cancellationToken = default);

    Task<PostDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
