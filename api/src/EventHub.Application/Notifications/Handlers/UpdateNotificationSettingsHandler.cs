using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

public sealed class UpdateNotificationSettingsHandler
    : ICommandHandler<UpdateNotificationSettingsCommand, NotificationSettingsDto>
{
    private readonly INotificationRepository _notifications;

    public UpdateNotificationSettingsHandler(INotificationRepository notifications) =>
        _notifications = notifications;

    public async Task<NotificationSettingsDto> HandleAsync(
        UpdateNotificationSettingsCommand command, CancellationToken cancellationToken = default)
    {
        var settings = await _notifications.GetSettingsAsync(command.UserId, cancellationToken);
        if (settings is null)
        {
            settings = NotificationSettings.CreateDefault(command.UserId);
            await _notifications.AddSettingsAsync(settings, cancellationToken);
        }

        settings.Update(
            command.EventReminders, command.WaitlistPromotions,
            command.HeartsEarned, command.NewComments);
        await _notifications.SaveChangesAsync(cancellationToken);

        return new NotificationSettingsDto(
            settings.EventReminders, settings.WaitlistPromotions,
            settings.HeartsEarned, settings.NewComments);
    }
}
