using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

public interface IPostReadRepository
{
    Task<IReadOnlyList<PostDto>> GetFeedAsync(
        Guid? currentUserId = null, CancellationToken cancellationToken = default);

    Task<PostDto?> GetByIdAsync(
        Guid id, Guid? currentUserId = null, CancellationToken cancellationToken = default);
}
