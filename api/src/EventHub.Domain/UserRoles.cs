namespace EventHub.Domain;

/// <summary>
/// Rôles applicatifs d'EventHub (un seul par utilisateur), gérés dans l'app ou
/// mappés depuis des groupes Entra. <c>student</c> est la valeur par défaut.
/// </summary>
public static class UserRoles
{
    public const string Student = "student";
    public const string Organizer = "organizer";
    public const string Moderator = "moderator";
    public const string Admin = "admin";

    public static readonly IReadOnlyList<string> All =
        new[] { Student, Organizer, Moderator, Admin };

    /// <summary>Vrai si <paramref name="role"/> est un rôle connu (casse ignorée).</summary>
    public static bool IsValid(string? role) =>
        role is not null && All.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

    /// <summary>Forme canonique (minuscules) d'un rôle valide, sinon <c>null</c>.</summary>
    public static string? Normalize(string? role) =>
        IsValid(role) ? role!.ToLowerInvariant() : null;
}
