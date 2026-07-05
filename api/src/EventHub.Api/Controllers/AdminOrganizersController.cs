using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;
using EventHub.Application.Organizers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/organizers")]
public class AdminOrganizersController : ControllerBase
{
    private readonly ISender _sender;

    public AdminOrganizersController(ISender sender) => _sender = sender;

    public sealed record OrganizerBody(string Name, string? ContactEmail);

    /// <summary>Liste des organisateurs (GET /api/admin/organizers).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrganizerDto>>> GetAll(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetOrganizersQuery(), cancellationToken));

    /// <summary>Créer un organisateur (POST /api/admin/organizers).</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] OrganizerBody body, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateOrganizerCommand(body.Name, body.ContactEmail), cancellationToken);
        return Created($"/api/admin/organizers/{result.Id}", new { id = result.Id });
    }

    /// <summary>Mettre à jour un organisateur (PATCH /api/admin/organizers/{id}).</summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] OrganizerBody body, CancellationToken cancellationToken)
    {
        var outcome = await _sender.Send(
            new UpdateOrganizerCommand(id, body.Name, body.ContactEmail), cancellationToken);
        return outcome == CrudOutcome.NotFound ? NotFound() : NoContent();
    }

    /// <summary>Supprimer un organisateur (DELETE /api/admin/organizers/{id}).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await _sender.Send(new DeleteOrganizerCommand(id), cancellationToken);
        return outcome == CrudOutcome.NotFound ? NotFound() : NoContent();
    }
}
