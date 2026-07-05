using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>Notification in-app destinée à un utilisateur (fil des notifications).</summary>
public class Notification : BaseEntity
{
    private Notification() { } // EF Core

    public Guid UserId { get; private set; }
    public string Type { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;

    /// <summary>Charge utile optionnelle (JSON) pour le deep-link côté app.</summary>
    public string? Data { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public static Notification Create(
        Guid userId, string type, string title, string body, string? data, DateTime nowUtc)
    {
        var notification = new Notification
        {
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            Type = Guard.AgainstNullOrWhiteSpace(type, nameof(type)),
            Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title)),
            Body = Guard.AgainstNullOrWhiteSpace(body, nameof(body)),
            Data = data,
        };
        notification.MarkCreated(nowUtc);
        return notification;
    }

    /// <summary>Marque comme lue (idempotent).</summary>
    public void MarkRead(DateTime nowUtc)
    {
        ReadAt ??= nowUtc;
        MarkUpdated(nowUtc);
    }
}
