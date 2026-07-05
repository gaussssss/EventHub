namespace EventHub.Domain.Services;

/// <summary>
/// Utilisateur de la requête courante. Alimenté par le claim Microsoft Entra
/// (`oid`) une fois l'auth activée ; repli sur en-tête en dev.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
}
