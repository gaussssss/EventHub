using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Gamification;

public sealed class GetGamificationSettingsHandler
    : IQueryHandler<GetGamificationSettingsQuery, GamificationSettingsDto>
{
    private readonly IGamificationSettingsRepository _settings;

    public GetGamificationSettingsHandler(IGamificationSettingsRepository settings) =>
        _settings = settings;

    public async Task<GamificationSettingsDto> HandleAsync(
        GetGamificationSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await _settings.GetAsync(cancellationToken);
        return settings is null
            ? new GamificationSettingsDto(0, 200, 500, 20)
            : new GamificationSettingsDto(
                0, settings.SilverThreshold, settings.GoldThreshold,
                settings.DefaultAttendanceReward);
    }
}
