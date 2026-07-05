using EventHub.Application.Attendance;
using EventHub.Application.Common.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "organizer,moderator,admin")]
// une fois l'authentification Microsoft Entra active.
[ApiController]
[Route("api/admin/activities/{activityId:guid}")]
public class AttendanceController : ControllerBase
{
    private readonly ISender _sender;

    public AttendanceController(ISender sender) => _sender = sender;

    public sealed record AttendanceRequest(List<Guid> UserIds);

    /// <summary>
    /// Marque des participants comme présents et crédite leurs cœurs
    /// (POST /api/admin/activities/{id}/attendance).
    /// </summary>
    [HttpPost("attendance")]
    public async Task<IActionResult> Mark(
        Guid activityId,
        [FromBody] AttendanceRequest? body,
        CancellationToken cancellationToken)
    {
        if (body is null || body.UserIds is null || body.UserIds.Count == 0)
            return BadRequest(new { error = "userIds requis" });

        var result = await _sender.Send(
            new MarkAttendanceCommand(activityId, body.UserIds), cancellationToken);

        return result.Status == AttendanceResultStatus.ActivityNotFound
            ? NotFound()
            : Ok(new { credited = result.Credited });
    }
}
