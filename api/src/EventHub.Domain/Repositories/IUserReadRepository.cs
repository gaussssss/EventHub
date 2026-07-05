using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

public interface IUserReadRepository
{
    Task<UserInfo?> GetAsync(Guid userId, CancellationToken cancellationToken = default);
}
