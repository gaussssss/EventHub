namespace EventHub.Domain.Services;

/// <summary>
/// Écritures « libre-service » sur le profil de l'utilisateur courant (nom,
/// avatar). Implémenté dans l'Infrastructure via <c>UserManager</c>.
/// </summary>
public interface IUserProfileService
{
    /// <summary>Renomme l'utilisateur. Renvoie <c>false</c> s'il n'existe pas.</summary>
    Task<bool> UpdateNameAsync(Guid userId, string name, CancellationToken cancellationToken = default);

    /// <summary>Met à jour l'URL d'avatar. Renvoie <c>false</c> s'il n'existe pas.</summary>
    Task<bool> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken = default);
}
