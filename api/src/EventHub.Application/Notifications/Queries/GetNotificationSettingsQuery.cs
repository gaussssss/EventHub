using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

/// <summary>Préférences de notification (GET /api/me/notification-settings).</summary>
public sealed record GetNotificationSettingsQuery(Guid UserId)
    : IQuery<NotificationSettingsDto>;
