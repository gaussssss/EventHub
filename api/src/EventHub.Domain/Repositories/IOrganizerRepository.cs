using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

public interface IOrganizerRepository
{
    Task<Organizer?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Organizer organizer, CancellationToken cancellationToken = default);

    void Remove(Organizer organizer);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
