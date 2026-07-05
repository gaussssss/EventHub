using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>Jeton de push d'un appareil de l'utilisateur (FCM/APNs).</summary>
public class Device : BaseEntity
{
    private Device() { } // EF Core

    public Guid UserId { get; private set; }
    public string PushToken { get; private set; } = null!;
    public string? Platform { get; private set; }

    public static Device Create(Guid userId, string pushToken, string? platform, DateTime nowUtc)
    {
        var device = new Device
        {
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            PushToken = Guard.AgainstNullOrWhiteSpace(pushToken, nameof(pushToken)),
            Platform = platform,
        };
        device.MarkCreated(nowUtc);
        return device;
    }
}
