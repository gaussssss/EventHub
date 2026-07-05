using EventHub.Application.Common.Messaging;
using EventHub.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/notifications")]
public class AdminNotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminNotificationsController(ISender sender) => _sender = sender;

    public sealed record BroadcastBody(string? Audience, string Title, string Body);

    /// <summary>Diffuser une notification (POST /api/admin/notifications/broadcast).</summary>
    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast(
        [FromBody] BroadcastBody body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Title) || string.IsNullOrWhiteSpace(body.Body))
            return BadRequest(new { error = "title et body requis" });

        var recipients = await _sender.Send(
            new BroadcastNotificationCommand(body.Audience ?? "all", body.Title, body.Body),
            cancellationToken);
        return Ok(new { recipients });
    }
}
