using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

public interface IOrganizerReadRepository
{
    Task<IReadOnlyList<OrganizerDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
