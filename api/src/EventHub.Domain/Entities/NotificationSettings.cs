using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Préférences de notification d'un utilisateur (clé = UserId). Toutes activées
/// par défaut.
/// </summary>
public class NotificationSettings
{
    private NotificationSettings() { } // EF Core

    public Guid UserId { get; private set; }
    public bool EventReminders { get; private set; } = true;
    public bool WaitlistPromotions { get; private set; } = true;
    public bool HeartsEarned { get; private set; } = true;
    public bool NewComments { get; private set; } = true;

    public static NotificationSettings CreateDefault(Guid userId) =>
        new() { UserId = Guard.AgainstEmpty(userId, nameof(userId)) };

    public void Update(
        bool eventReminders, bool waitlistPromotions, bool heartsEarned, bool newComments)
    {
        EventReminders = eventReminders;
        WaitlistPromotions = waitlistPromotions;
        HeartsEarned = heartsEarned;
        NewComments = newComments;
    }
}
