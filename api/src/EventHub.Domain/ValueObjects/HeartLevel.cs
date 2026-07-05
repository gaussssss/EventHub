namespace EventHub.Domain.ValueObjects;

/// <summary>
/// Niveau de gamification « cœurs santé ». Autorité serveur, alignée avec le
/// calcul de l'app mobile : Bronze &lt; 200, Argent &lt; 500, Or &gt;= 500.
/// </summary>
public sealed record HeartLevel(string Name, int PreviousThreshold, int NextThreshold)
{
    public static HeartLevel FromHearts(int totalHearts)
    {
        if (totalHearts >= 500) return new HeartLevel("Or", 500, 1000);
        if (totalHearts >= 200) return new HeartLevel("Argent", 200, 500);
        return new HeartLevel("Bronze", 0, 200);
    }
}
