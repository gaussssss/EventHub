using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

public sealed class GetNotificationSettingsHandler
    : IQueryHandler<GetNotificationSettingsQuery, NotificationSettingsDto>
{
    private readonly INotificationRepository _notifications;

    public GetNotificationSettingsHandler(INotificationRepository notifications) =>
        _notifications = notifications;

    public async Task<NotificationSettingsDto> HandleAsync(
        GetNotificationSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await _notifications.GetSettingsAsync(query.UserId, cancellationToken);

        // Aucune préférence enregistrée → tout activé par défaut.
        return settings is null
            ? new NotificationSettingsDto(true, true, true, true)
            : new NotificationSettingsDto(
                settings.EventReminders, settings.WaitlistPromotions,
                settings.HeartsEarned, settings.NewComments);
    }
}
