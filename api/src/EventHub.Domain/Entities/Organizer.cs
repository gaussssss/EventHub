using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>Organisateur d'activités (association, service UQTR…).</summary>
public class Organizer : BaseEntity
{
    private Organizer() { } // EF Core

    public string Name { get; private set; } = null!;
    public string? ContactEmail { get; private set; }

    public static Organizer Create(string name, string? contactEmail = null)
    {
        return new Organizer
        {
            Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)),
            ContactEmail = contactEmail,
        };
    }

    public void Update(string name, string? contactEmail)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        ContactEmail = contactEmail;
    }
}
