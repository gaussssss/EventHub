namespace EventHub.Domain.Services;

/// <summary>
/// Écritures liées aux utilisateurs pour le back office : rôle (via Identity)
/// et statut de compte. Implémenté dans l'Infrastructure avec UserManager/RoleManager.
/// </summary>
public interface IUserAdminService
{
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Remplace le rôle unique de l'utilisateur (le rôle doit exister).</summary>
    Task SetRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);

    /// <summary>Change le statut du compte (active | suspended | deleted).</summary>
    Task SetStatusAsync(Guid userId, string status, CancellationToken cancellationToken = default);
}
