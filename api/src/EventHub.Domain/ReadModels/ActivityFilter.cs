namespace EventHub.Domain.ReadModels;

/// <summary>
/// Critères de filtrage du catalogue d'activités (GET /api/activities).
/// POCO d'entrée du read-repo — reste sans dépendance côté Domaine.
/// </summary>
public sealed record ActivityFilter
{
    /// <summary>Restreindre aux activités « à la une » (carrousel).</summary>
    public bool FeaturedOnly { get; init; }

    /// <summary>Slug de catégorie (ex. « sport »).</summary>
    public string? Category { get; init; }

    /// <summary>Recherche plein-texte sur le titre et le lieu.</summary>
    public string? Search { get; init; }

    /// <summary>Ne garder que les activités avec au moins une place libre.</summary>
    public bool AvailableOnly { get; init; }

    /// <summary>Borne basse sur <c>StartsAt</c> (incluse).</summary>
    public DateTime? From { get; init; }

    /// <summary>Borne haute sur <c>StartsAt</c> (incluse).</summary>
    public DateTime? To { get; init; }

    /// <summary>Tri décroissant par date de début (défaut : croissant).</summary>
    public bool Descending { get; init; }
}
