using EventHub.Domain.Entities;
using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

/// <summary>
/// Crée une notification in-app par destinataire puis déclenche un push (stub
/// tant que la config FCM/APNs n'est pas fournie).
/// </summary>
public sealed class BroadcastNotificationHandler
    : ICommandHandler<BroadcastNotificationCommand, int>
{
    private const string BroadcastType = "broadcast";

    private readonly INotificationRepository _notifications;
    private readonly IPushSender _push;
    private readonly IClock _clock;

    public BroadcastNotificationHandler(
        INotificationRepository notifications, IPushSender push, IClock clock)
    {
        _notifications = notifications;
        _push = push;
        _clock = clock;
    }

    public async Task<int> HandleAsync(
        BroadcastNotificationCommand command, CancellationToken cancellationToken = default)
    {
        var recipientIds = await _notifications.GetBroadcastRecipientIdsAsync(
            command.Audience ?? "all", cancellationToken);
        if (recipientIds.Count == 0)
            return 0;

        var now = _clock.UtcNow;
        var notifications = recipientIds
            .Select(id => Notification.Create(
                id, BroadcastType, command.Title, command.Body, null, now))
            .ToList();

        await _notifications.AddRangeAsync(notifications, cancellationToken);
        await _notifications.SaveChangesAsync(cancellationToken);

        foreach (var id in recipientIds)
            await _push.SendAsync(id, command.Title, command.Body, cancellationToken);

        return recipientIds.Count;
    }
}
