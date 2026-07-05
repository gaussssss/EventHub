using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Gamification;

/// <summary>Mettre à jour les réglages (PATCH /api/admin/settings/gamification).</summary>
public sealed record UpdateGamificationSettingsCommand(
    int SilverThreshold, int GoldThreshold, int DefaultAttendanceReward)
    : ICommand<GamificationSettingsDto>;
