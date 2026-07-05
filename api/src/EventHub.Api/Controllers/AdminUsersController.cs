using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly ISender _sender;

    public AdminUsersController(ISender sender) => _sender = sender;

    public sealed record UpdateUserRequest(string? Role, string? Status);
    public sealed record AdjustHeartsRequest(int Hearts, string? Reason);

    /// <summary>Rechercher des utilisateurs (GET /api/admin/users?q=).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> Search(
        [FromQuery] string? q, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new SearchUsersQuery(q), cancellationToken));

    /// <summary>Changer rôle et/ou statut (PATCH /api/admin/users/{id}).</summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateUserRequest body, CancellationToken cancellationToken)
    {
        var status = await _sender.Send(
            new UpdateUserCommand(id, body.Role, body.Status), cancellationToken);

        return status switch
        {
            UpdateUserStatus.Updated => NoContent(),
            UpdateUserStatus.NotFound => NotFound(),
            UpdateUserStatus.InvalidRole => BadRequest(new { error = "role invalide" }),
            UpdateUserStatus.InvalidStatus => BadRequest(new { error = "status invalide" }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    /// <summary>Ajustement manuel de cœurs (POST /api/admin/users/{id}/hearts).</summary>
    [HttpPost("{id:guid}/hearts")]
    public async Task<IActionResult> AdjustHearts(
        Guid id, [FromBody] AdjustHeartsRequest body, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AdjustHeartsCommand(id, body.Hearts, body.Reason ?? "admin_adjust"), cancellationToken);

        return result.Status switch
        {
            AdjustHeartsStatus.Adjusted => Ok(new { totalHearts = result.NewTotal }),
            AdjustHeartsStatus.UserNotFound => NotFound(),
            AdjustHeartsStatus.InvalidAmount => BadRequest(new { error = "hearts doit être non nul" }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }
}
