using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

public interface IHeartReadRepository
{
    Task<int> GetTotalAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HeartHistoryDto>> GetHistoryAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
