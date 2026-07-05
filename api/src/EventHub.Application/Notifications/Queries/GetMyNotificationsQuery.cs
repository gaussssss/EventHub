using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

/// <summary>Fil des notifications de l'utilisateur (GET /api/me/notifications).</summary>
public sealed record GetMyNotificationsQuery(Guid UserId)
    : IQuery<IReadOnlyList<NotificationDto>>;
