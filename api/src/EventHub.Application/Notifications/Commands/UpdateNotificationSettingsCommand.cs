using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

/// <summary>Mettre à jour ses préférences (PATCH /api/me/notification-settings).</summary>
public sealed record UpdateNotificationSettingsCommand(
    Guid UserId,
    bool EventReminders,
    bool WaitlistPromotions,
    bool HeartsEarned,
    bool NewComments)
    : ICommand<NotificationSettingsDto>;
