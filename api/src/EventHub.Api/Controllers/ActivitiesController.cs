using EventHub.Domain.ReadModels;
using EventHub.Application.Activities;
using EventHub.Application.Common.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly ISender _sender;

    public ActivitiesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Catalogue filtré des activités publiées (GET /api/activities).
    /// Filtres : <c>?category=</c> (slug), <c>?q=</c> (titre/lieu),
    /// <c>?availableOnly=true</c>, <c>?from=</c>/<c>?to=</c> (ISO-8601),
    /// <c>?sort=-date</c> (décroissant ; croissant par défaut).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivityDto>>> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? q,
        [FromQuery] bool availableOnly,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var filter = new ActivityFilter
        {
            Category = category,
            Search = q,
            AvailableOnly = availableOnly,
            From = from,
            To = to,
            Descending = sort is "-date" or "date_desc",
        };
        return Ok(await _sender.Send(new GetActivitiesQuery(filter), cancellationToken));
    }

    /// <summary>Les activités « à la une » (GET /api/activities/featured).</summary>
    [HttpGet("featured")]
    public async Task<ActionResult<IReadOnlyList<ActivityDto>>> GetFeatured(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new GetActivitiesQuery(new ActivityFilter { FeaturedOnly = true }),
            cancellationToken));

    /// <summary>Détail d'une activité (GET /api/activities/{id}).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var activity = await _sender.Send(new GetActivityByIdQuery(id), cancellationToken);
        return activity is null ? NotFound() : Ok(activity);
    }
}
