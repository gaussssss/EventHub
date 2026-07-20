using EventHub.Application.Admin;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.Enums;
using EventHub.Domain.ReadModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "organizer,admin")]
[ApiController]
[Route("api/admin/activities")]
public class AdminActivitiesController : ControllerBase
{
    private readonly ISender _sender;

    public AdminActivitiesController(ISender sender) => _sender = sender;

    public sealed record ActivityBody(
        string Title,
        string Description,
        Guid CategoryId,
        Guid? OrganizerId,
        DateTime StartsAt,
        DateTime? EndsAt,
        string Location,
        string ImageUrl,
        int HeartsReward,
        int MaxParticipants,
        string? RegistrationUrl,
        DateTime? RegistrationDeadline,
        bool IsFeatured,
        string? Status,
        decimal ParticipationCost = 0m);

    private static ActivityStatus ParseStatus(string? status) =>
        Enum.TryParse<ActivityStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : ActivityStatus.Published;

    /// <summary>Liste des activités, tous statuts (GET /api/admin/activities).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminActivityDto>>> GetAll(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAdminActivitiesQuery(), cancellationToken));

    /// <summary>Détail complet d'une activité (GET /api/admin/activities/{id}).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminActivityDetailDto>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var detail = await _sender.Send(new GetAdminActivityQuery(id), cancellationToken);
        return detail is null ? NotFound() : Ok(detail);
    }

    /// <summary>Créer une activité (POST /api/admin/activities).</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ActivityBody body, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateActivityCommand(
            body.Title, body.Description, body.CategoryId, body.OrganizerId,
            body.StartsAt, body.EndsAt, body.Location, body.ImageUrl,
            body.HeartsReward, body.MaxParticipants, body.RegistrationUrl,
            body.RegistrationDeadline, body.IsFeatured, ParseStatus(body.Status),
            body.ParticipationCost),
            cancellationToken);

        return result.Status == CreateActivityStatus.CategoryNotFound
            ? BadRequest(new { error = "categoryId inconnu" })
            : Created($"/api/activities/{result.ActivityId}", new { id = result.ActivityId });
    }

    /// <summary>Mettre à jour une activité (PUT /api/admin/activities/{id}).</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] ActivityBody body, CancellationToken cancellationToken)
    {
        var status = await _sender.Send(new UpdateActivityCommand(
            id, body.Title, body.Description, body.CategoryId, body.OrganizerId,
            body.StartsAt, body.EndsAt, body.Location, body.ImageUrl,
            body.HeartsReward, body.MaxParticipants, body.RegistrationUrl,
            body.RegistrationDeadline, body.IsFeatured, ParseStatus(body.Status),
            body.ParticipationCost),
            cancellationToken);

        return status switch
        {
            UpdateActivityStatus.Updated => NoContent(),
            UpdateActivityStatus.NotFound => NotFound(),
            UpdateActivityStatus.CategoryNotFound => BadRequest(new { error = "categoryId inconnu" }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    /// <summary>Publier une activité (POST /api/admin/activities/{id}/publish).</summary>
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
        => ToResult(await _sender.Send(new PublishActivityCommand(id), cancellationToken));

    /// <summary>Annuler une activité (POST /api/admin/activities/{id}/cancel).</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => ToResult(await _sender.Send(new CancelActivityCommand(id), cancellationToken));

    /// <summary>Basculer « à la une » (POST /api/admin/activities/{id}/feature).</summary>
    [HttpPost("{id:guid}/feature")]
    public async Task<IActionResult> Feature(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ToggleFeatureCommand(id), cancellationToken);
        return result.Status == ActivityActionStatus.NotFound
            ? NotFound()
            : Ok(new { isFeatured = result.IsFeatured });
    }

    /// <summary>Inscrits + liste d'attente (GET /api/admin/activities/{id}/registrations).</summary>
    [HttpGet("{id:guid}/registrations")]
    public async Task<IActionResult> Registrations(Guid id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetActivityRegistrationsQuery(id), cancellationToken));

    // NB : POST /api/admin/activities/{id}/attendance est fourni par AttendanceController.

    private IActionResult ToResult(ActivityActionStatus status) =>
        status == ActivityActionStatus.Done ? NoContent() : NotFound();
}
