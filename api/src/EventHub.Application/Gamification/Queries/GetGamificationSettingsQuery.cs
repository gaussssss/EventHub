using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Gamification;

/// <summary>Lire les réglages de gamification (GET /api/admin/settings/gamification).</summary>
public sealed record GetGamificationSettingsQuery : IQuery<GamificationSettingsDto>;
