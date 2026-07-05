using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Réglages de gamification (singleton back-office) : seuils de niveaux et
/// récompense de présence par défaut. Note : l'app mobile calcule aujourd'hui
/// le niveau avec des seuils figés (200/500) — ces valeurs sont la référence
/// serveur, destinées à piloter le calcul quand l'app les consommera.
/// </summary>
public class GamificationSettings
{
    /// <summary>Identifiant unique de la ligne de configuration.</summary>
    public static readonly Guid SingletonId = new("11111111-1111-1111-1111-111111111111");

    private GamificationSettings() { } // EF Core

    public Guid Id { get; private set; } = SingletonId;

    public int SilverThreshold { get; private set; } = 200;
    public int GoldThreshold { get; private set; } = 500;
    public int DefaultAttendanceReward { get; private set; } = 20;

    public static GamificationSettings CreateDefault() => new();

    public void Update(int silverThreshold, int goldThreshold, int defaultAttendanceReward)
    {
        SilverThreshold = Guard.AgainstNegative(silverThreshold, nameof(silverThreshold));
        GoldThreshold = Guard.AgainstNegative(goldThreshold, nameof(goldThreshold));
        DefaultAttendanceReward =
            Guard.AgainstNegative(defaultAttendanceReward, nameof(defaultAttendanceReward));

        if (goldThreshold <= silverThreshold)
            throw new DomainException("Le seuil Or doit être supérieur au seuil Argent.");
    }
}
