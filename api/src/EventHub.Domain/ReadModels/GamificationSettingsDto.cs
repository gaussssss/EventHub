namespace EventHub.Domain.ReadModels;

/// <summary>Réglages de gamification exposés au back-office.</summary>
public sealed record GamificationSettingsDto(
    int BronzeThreshold,
    int SilverThreshold,
    int GoldThreshold,
    int DefaultAttendanceReward);
