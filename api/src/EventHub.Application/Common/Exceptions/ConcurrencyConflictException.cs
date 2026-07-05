namespace EventHub.Application.Common.Exceptions;

/// <summary>
/// Levée quand une écriture échoue le contrôle de concurrence optimiste
/// (jeton de version modifié entre-temps). Le cas d'usage peut rejouer l'opération.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("Conflit de concurrence : la ressource a été modifiée entre-temps.")
    {
    }
}
