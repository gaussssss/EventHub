using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Activities;

/// <summary>
/// Requête « catalogue d'activités » : filtre optionnel par catégorie, recherche
/// titre/lieu, intervalle de dates, places disponibles, tri, et sélecteur
/// « à la une ». Sans filtre, renvoie toutes les activités publiées.
/// </summary>
public sealed record GetActivitiesQuery(ActivityFilter Filter)
    : IQuery<IReadOnlyList<ActivityDto>>
{
    public GetActivitiesQuery() : this(new ActivityFilter()) { }
}
