using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Contributeur du projet, affiché sur la page « À propos » de l'app mobile et
/// géré depuis le back office (Paramètres).
/// </summary>
public class Contributor : BaseEntity
{
    private Contributor() { } // EF Core

    public string Name { get; private set; } = null!;

    /// <summary>Rôle affiché (ex. « Développement mobile », « Design »).</summary>
    public string Role { get; private set; } = null!;

    public string? AvatarUrl { get; private set; }

    /// <summary>Ordre d'affichage croissant sur la page « À propos ».</summary>
    public int SortOrder { get; private set; }

    public static Contributor Create(
        string name, string role, string? avatarUrl, int sortOrder)
    {
        return new Contributor
        {
            Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)),
            Role = Guard.AgainstNullOrWhiteSpace(role, nameof(role)),
            AvatarUrl = avatarUrl,
            SortOrder = sortOrder,
        };
    }

    public void Update(string name, string role, string? avatarUrl, int sortOrder)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Role = Guard.AgainstNullOrWhiteSpace(role, nameof(role));
        AvatarUrl = avatarUrl;
        SortOrder = sortOrder;
    }
}
