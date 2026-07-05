using EventHub.Domain.Services;
using EventHub.Application.Activities;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Registrations;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/activities/{activityId:guid}")]
public class RegistrationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;
    private readonly IConfiguration _configuration;

    public RegistrationsController(
        ISender sender, ICurrentUser currentUser, IConfiguration configuration)
    {
        _sender = sender;
        _currentUser = currentUser;
        _configuration = configuration;
    }

    public sealed record RegisterRequest(string? FormResponseId);

    /// <summary>S'inscrire à une activité (POST /api/activities/{id}/register).</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        Guid activityId,
        [FromBody] RegisterRequest? body,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var result = await _sender.Send(
            new RegisterForActivityCommand(userId.Value, activityId, body?.FormResponseId),
            cancellationToken);

        return result.Status switch
        {
            RegistrationResultStatus.Registered =>
                Ok(new { status = "registered" }),
            RegistrationResultStatus.Waitlisted =>
                Ok(new { status = "waitlisted" }),
            RegistrationResultStatus.AlreadyRegistered =>
                Ok(new { status = "alreadyRegistered" }),
            RegistrationResultStatus.ActivityNotFound =>
                NotFound(),
            RegistrationResultStatus.Rejected =>
                Conflict(new { status = "rejected", reason = result.Reason?.ToString() }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    /// <summary>Annuler son inscription (POST /api/activities/{id}/cancel).</summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(Guid activityId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var result = await _sender.Send(
            new CancelRegistrationCommand(userId.Value, activityId), cancellationToken);

        return result.Status switch
        {
            CancellationResultStatus.Cancelled =>
                Ok(new { status = "cancelled", promotedUserId = result.PromotedUserId }),
            CancellationResultStatus.NotRegistered =>
                NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    /// <summary>Statut d'inscription de l'utilisateur (GET /api/activities/{id}/registration).</summary>
    [HttpGet("registration")]
    public async Task<IActionResult> Status(Guid activityId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        return Ok(await _sender.Send(
            new GetRegistrationStatusQuery(userId.Value, activityId), cancellationToken));
    }

    /// <summary>Lien du formulaire d'inscription (GET /api/activities/{id}/registration-url).</summary>
    [HttpGet("registration-url")]
    public async Task<IActionResult> RegistrationUrl(
        Guid activityId, CancellationToken cancellationToken)
    {
        var activity = await _sender.Send(new GetActivityByIdQuery(activityId), cancellationToken);
        return activity is null ? NotFound() : Ok(new { url = activity.RegistrationUrl });
    }

    /// <summary>Lien de partage / deep link (GET /api/activities/{id}/share).</summary>
    [HttpGet("share")]
    public async Task<IActionResult> Share(Guid activityId, CancellationToken cancellationToken)
    {
        var activity = await _sender.Send(new GetActivityByIdQuery(activityId), cancellationToken);
        if (activity is null)
            return NotFound();

        var baseUrl = _configuration["App:ShareBaseUrl"] ?? "https://eventhub.uqtr.ca";
        return Ok(new
        {
            shareUrl = $"{baseUrl.TrimEnd('/')}/activities/{activityId}",
            title = activity.Title,
        });
    }
}
