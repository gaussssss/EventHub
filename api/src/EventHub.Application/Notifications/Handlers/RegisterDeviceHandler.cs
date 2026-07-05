using EventHub.Domain.Entities;
using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

public sealed class RegisterDeviceHandler : ICommandHandler<RegisterDeviceCommand, Guid>
{
    private readonly INotificationRepository _notifications;
    private readonly IClock _clock;

    public RegisterDeviceHandler(INotificationRepository notifications, IClock clock)
    {
        _notifications = notifications;
        _clock = clock;
    }

    public async Task<Guid> HandleAsync(
        RegisterDeviceCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _notifications.FindDeviceByTokenAsync(
            command.PushToken, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var device = Device.Create(
            command.UserId, command.PushToken, command.Platform, _clock.UtcNow);
        await _notifications.AddDeviceAsync(device, cancellationToken);
        await _notifications.SaveChangesAsync(cancellationToken);
        return device.Id;
    }
}
