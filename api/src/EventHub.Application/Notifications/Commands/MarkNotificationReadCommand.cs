using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

/// <summary>Marquer une notification comme lue (PATCH /api/me/notifications/{id}/read).</summary>
public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId)
    : ICommand<MarkReadStatus>;
