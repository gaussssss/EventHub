namespace EventHub.Domain.Common;

/// <summary>Violation d'un invariant métier (agrégat construit ou muté dans un état invalide).</summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
