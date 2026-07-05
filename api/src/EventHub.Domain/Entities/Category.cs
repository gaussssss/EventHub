using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>Catégorie d'activité (slug: "sport" | "socioculturel" | ...).</summary>
public class Category : BaseEntity
{
    private Category() { } // EF Core

    public string Slug { get; private set; } = null!;
    public string Label { get; private set; } = null!;
    public string? Color { get; private set; }
    public string? Icon { get; private set; }

    public static Category Create(string slug, string label, string? color = null, string? icon = null)
    {
        return new Category
        {
            Slug = Guard.AgainstNullOrWhiteSpace(slug, nameof(slug)),
            Label = Guard.AgainstNullOrWhiteSpace(label, nameof(label)),
            Color = color,
            Icon = icon,
        };
    }

    public void Update(string slug, string label, string? color, string? icon)
    {
        Slug = Guard.AgainstNullOrWhiteSpace(slug, nameof(slug));
        Label = Guard.AgainstNullOrWhiteSpace(label, nameof(label));
        Color = color;
        Icon = icon;
    }
}
