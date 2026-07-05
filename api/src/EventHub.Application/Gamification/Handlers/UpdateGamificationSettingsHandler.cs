using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Gamification;

public sealed class UpdateGamificationSettingsHandler
    : ICommandHandler<UpdateGamificationSettingsCommand, GamificationSettingsDto>
{
    private readonly IGamificationSettingsRepository _settings;

    public UpdateGamificationSettingsHandler(IGamificationSettingsRepository settings) =>
        _settings = settings;

    public async Task<GamificationSettingsDto> HandleAsync(
        UpdateGamificationSettingsCommand command, CancellationToken cancellationToken = default)
    {
        var settings = await _settings.GetAsync(cancellationToken);
        if (settings is null)
        {
            settings = GamificationSettings.CreateDefault();
            await _settings.AddAsync(settings, cancellationToken);
        }

        settings.Update(
            command.SilverThreshold, command.GoldThreshold, command.DefaultAttendanceReward);
        await _settings.SaveChangesAsync(cancellationToken);

        return new GamificationSettingsDto(
            0, settings.SilverThreshold, settings.GoldThreshold, settings.DefaultAttendanceReward);
    }
}
