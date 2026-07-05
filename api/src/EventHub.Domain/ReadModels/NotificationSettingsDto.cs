namespace EventHub.Domain.ReadModels;

/// <summary>Préférences de notification (GET/PATCH /api/me/notification-settings).</summary>
public sealed record NotificationSettingsDto(
    bool EventReminders,
    bool WaitlistPromotions,
    bool HeartsEarned,
    bool NewComments);
