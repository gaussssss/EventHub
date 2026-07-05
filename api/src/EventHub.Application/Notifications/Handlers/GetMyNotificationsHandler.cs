using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

public sealed class GetMyNotificationsHandler
    : IQueryHandler<GetMyNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationReadRepository _notifications;

    public GetMyNotificationsHandler(INotificationReadRepository notifications) =>
        _notifications = notifications;

    public Task<IReadOnlyList<NotificationDto>> HandleAsync(
        GetMyNotificationsQuery query, CancellationToken cancellationToken = default) =>
        _notifications.GetForUserAsync(query.UserId, cancellationToken);
}
