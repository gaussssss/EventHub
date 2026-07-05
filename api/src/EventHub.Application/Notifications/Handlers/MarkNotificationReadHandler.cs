using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

public sealed class MarkNotificationReadHandler
    : ICommandHandler<MarkNotificationReadCommand, MarkReadStatus>
{
    private readonly INotificationRepository _notifications;
    private readonly IClock _clock;

    public MarkNotificationReadHandler(INotificationRepository notifications, IClock clock)
    {
        _notifications = notifications;
        _clock = clock;
    }

    public async Task<MarkReadStatus> HandleAsync(
        MarkNotificationReadCommand command, CancellationToken cancellationToken = default)
    {
        var notification = await _notifications.GetAsync(command.NotificationId, cancellationToken);
        if (notification is null)
            return MarkReadStatus.NotFound;

        if (notification.UserId != command.UserId)
            return MarkReadStatus.Forbidden;

        notification.MarkRead(_clock.UtcNow);
        await _notifications.SaveChangesAsync(cancellationToken);
        return MarkReadStatus.Marked;
    }
}
