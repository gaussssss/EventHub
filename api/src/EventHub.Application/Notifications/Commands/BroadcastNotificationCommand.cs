using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

/// <summary>
/// Diffuser une notification à une audience (POST /api/admin/notifications/broadcast).
/// Renvoie le nombre de destinataires.
/// </summary>
public sealed record BroadcastNotificationCommand(string Audience, string Title, string Body)
    : ICommand<int>;
