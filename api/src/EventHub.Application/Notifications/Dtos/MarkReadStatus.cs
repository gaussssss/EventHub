namespace EventHub.Application.Notifications;

/// <summary>Issue du marquage d'une notification comme lue.</summary>
public enum MarkReadStatus
{
    Marked,
    NotFound,
    Forbidden,
}
