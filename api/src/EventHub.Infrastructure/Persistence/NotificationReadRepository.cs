using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class NotificationReadRepository : INotificationReadRepository
{
    private readonly EventHubDbContext _db;

    public NotificationReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Body = n.Body,
                Data = n.Data,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt,
            })
            .ToListAsync(cancellationToken);
}
