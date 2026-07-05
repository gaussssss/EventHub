using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

/// <summary>Accès à la configuration de gamification (singleton).</summary>
public interface IGamificationSettingsRepository
{
    Task<GamificationSettings?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(GamificationSettings settings, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
