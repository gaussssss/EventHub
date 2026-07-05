using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Moderation;
using EventHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public ReportsController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    public sealed record ReportRequest(string TargetType, Guid TargetId, string Reason);

    /// <summary>Signaler un post ou un commentaire (POST /api/reports).</summary>
    [HttpPost]
    public async Task<IActionResult> Report(
        [FromBody] ReportRequest body, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        if (!Enum.TryParse<ReportTargetType>(body.TargetType, ignoreCase: true, out var targetType))
            return BadRequest(new { error = "targetType invalide (post|comment)" });
        if (string.IsNullOrWhiteSpace(body.Reason))
            return BadRequest(new { error = "reason requis" });

        var result = await _sender.Send(
            new ReportContentCommand(userId.Value, targetType, body.TargetId, body.Reason),
            cancellationToken);

        return result.Status == ReportContentStatus.TargetNotFound
            ? NotFound(new { error = "contenu introuvable" })
            : StatusCode(StatusCodes.Status201Created, new { id = result.ReportId });
    }
}
