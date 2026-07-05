using EventHub.Domain.ReadModels;
using EventHub.Application.Categories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/categories")]
public class AdminCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public AdminCategoriesController(ISender sender) => _sender = sender;

    public sealed record CategoryBody(string Slug, string Label, string? Color, string? Icon);

    /// <summary>Liste des catégories (GET /api/admin/categories).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetCategoriesQuery(), cancellationToken));

    /// <summary>Créer une catégorie (POST /api/admin/categories).</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CategoryBody body, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateCategoryCommand(body.Slug, body.Label, body.Color, body.Icon),
            cancellationToken);

        return result.Outcome == CrudOutcome.Conflict
            ? Conflict(new { error = "slug déjà utilisé" })
            : Created($"/api/categories/{result.Id}", new { id = result.Id });
    }

    /// <summary>Mettre à jour une catégorie (PATCH /api/admin/categories/{id}).</summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] CategoryBody body, CancellationToken cancellationToken)
    {
        var outcome = await _sender.Send(
            new UpdateCategoryCommand(id, body.Slug, body.Label, body.Color, body.Icon),
            cancellationToken);

        return outcome switch
        {
            CrudOutcome.Done => NoContent(),
            CrudOutcome.NotFound => NotFound(),
            CrudOutcome.Conflict => Conflict(new { error = "slug déjà utilisé" }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    /// <summary>Supprimer une catégorie (DELETE /api/admin/categories/{id}).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await _sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        return outcome == CrudOutcome.NotFound ? NotFound() : NoContent();
    }
}
