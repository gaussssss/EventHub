using Microsoft.AspNetCore.Identity;

namespace EventHub.Infrastructure.Identity;

/// <summary>
/// Utilisateur ASP.NET Core Identity (clé Guid). Alimenté au premier login
/// Microsoft Entra (nom, courriel, avatar via Graph) — config fournie plus tard.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        // Stamp par défaut : rend l'utilisateur valide pour les opérations
        // UserManager (rôles, statut) même s'il est créé hors UserManager
        // (provisioning JIT / seed). Écrasé par la valeur stockée à la lecture EF.
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }

    /// <summary>active | suspended | deleted.</summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Identifiant d'objet Microsoft Entra (claim <c>oid</c>). Renseigné au
    /// provisioning JIT du premier login ; sert à retrouver l'utilisateur interne
    /// à partir du jeton. <c>null</c> pour les comptes créés hors SSO (seed/dev).
    /// </summary>
    public string? EntraObjectId { get; set; }
}
