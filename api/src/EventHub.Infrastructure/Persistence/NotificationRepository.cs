using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly EventHubDbContext _db;

    public NotificationRepository(EventHubDbContext db) => _db = db;

    public Task<Notification?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
        await _db.Notifications.AddAsync(notification, cancellationToken);

    public async Task AddRangeAsync(
        IEnumerable<Notification> notifications, CancellationToken cancellationToken = default) =>
        await _db.Notifications.AddRangeAsync(notifications, cancellationToken);

    public Task<Device?> FindDeviceByTokenAsync(
        string pushToken, CancellationToken cancellationToken = default) =>
        _db.Devices.FirstOrDefaultAsync(d => d.PushToken == pushToken, cancellationToken);

    public async Task AddDeviceAsync(Device device, CancellationToken cancellationToken = default) =>
        await _db.Devices.AddAsync(device, cancellationToken);

    public Task<NotificationSettings?> GetSettingsAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        _db.NotificationSettings.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

    public async Task AddSettingsAsync(
        NotificationSettings settings, CancellationToken cancellationToken = default) =>
        await _db.NotificationSettings.AddAsync(settings, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetBroadcastRecipientIdsAsync(
        string audience, CancellationToken cancellationToken = default) =>
        // Audience « all » (défaut) = tous les utilisateurs. D'autres segments
        // (par rôle, par inscription…) pourront être ajoutés ici.
        await _db.Users.Select(u => u.Id).ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
