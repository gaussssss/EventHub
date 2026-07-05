namespace EventHub.Domain.Common;

/// <summary>Gardes d'invariants : lèvent une <see cref="DomainException"/> si violés.</summary>
public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} est requis.");
        return value;
    }

    public static int AgainstNegative(int value, string name)
    {
        if (value < 0)
            throw new DomainException($"{name} ne peut pas être négatif.");
        return value;
    }

    public static int AgainstNonPositive(int value, string name)
    {
        if (value <= 0)
            throw new DomainException($"{name} doit être strictement positif.");
        return value;
    }

    public static Guid AgainstEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
            throw new DomainException($"{name} est requis.");
        return value;
    }
}
