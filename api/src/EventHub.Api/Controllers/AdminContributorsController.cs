using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;
using EventHub.Application.Contributors;
using EventHub.Domain.ReadModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

/// <summary>Gestion des contributeurs de la page « À propos » (back office).</summary>
[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/contributors")]
public class AdminContributorsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminContributorsController(ISender sender) => _sender = sender;

    public sealed record ContributorBody(
        string Name, string Role, string? AvatarUrl, int SortOrder);

    /// <summary>Liste des contributeurs (GET /api/admin/contributors).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContributorDto>>> GetAll(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetContributorsQuery(), cancellationToken));

    /// <summary>Créer un contributeur (POST /api/admin/contributors).</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ContributorBody body, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateContributorCommand(
                body.Name, body.Role, body.AvatarUrl, body.SortOrder),
            cancellationToken);
        return Created($"/api/admin/contributors/{result.Id}", new { id = result.Id });
    }

    /// <summary>Mettre à jour un contributeur (PATCH /api/admin/contributors/{id}).</summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] ContributorBody body, CancellationToken cancellationToken)
    {
        var outcome = await _sender.Send(
            new UpdateContributorCommand(
                id, body.Name, body.Role, body.AvatarUrl, body.SortOrder),
            cancellationToken);
        return outcome == CrudOutcome.NotFound ? NotFound() : NoContent();
    }

    /// <summary>Supprimer un contributeur (DELETE /api/admin/contributors/{id}).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await _sender.Send(new DeleteContributorCommand(id), cancellationToken);
        return outcome == CrudOutcome.NotFound ? NotFound() : NoContent();
    }
}
