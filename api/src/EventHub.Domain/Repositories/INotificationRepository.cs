using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

/// <summary>Écritures des notifications, appareils et préférences.</summary>
public interface INotificationRepository
{
    Task<Notification?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task AddRangeAsync(
        IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);

    Task<Device?> FindDeviceByTokenAsync(
        string pushToken, CancellationToken cancellationToken = default);
    Task AddDeviceAsync(Device device, CancellationToken cancellationToken = default);

    Task<NotificationSettings?> GetSettingsAsync(
        Guid userId, CancellationToken cancellationToken = default);
    Task AddSettingsAsync(
        NotificationSettings settings, CancellationToken cancellationToken = default);

    /// <summary>Ids des destinataires d'une diffusion (audience « all » = tous).</summary>
    Task<IReadOnlyList<Guid>> GetBroadcastRecipientIdsAsync(
        string audience, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
