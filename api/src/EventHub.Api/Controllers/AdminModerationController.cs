using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "moderator,admin")]
[ApiController]
[Route("api/admin")]
public class AdminModerationController : ControllerBase
{
    private readonly ISender _sender;

    public AdminModerationController(ISender sender) => _sender = sender;

    /// <summary>File des signalements ouverts (GET /api/admin/reports).</summary>
    [HttpGet("reports")]
    public async Task<ActionResult<IReadOnlyList<ReportDto>>> Reports(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetOpenReportsQuery(), cancellationToken));

    /// <summary>Masquer une publication (POST /api/admin/posts/{id}/hide).</summary>
    [HttpPost("posts/{id:guid}/hide")]
    public async Task<IActionResult> HidePost(Guid id, CancellationToken cancellationToken)
        => await _sender.Send(new HidePostCommand(id), cancellationToken) == HideResult.NotFound
            ? NotFound()
            : NoContent();

    /// <summary>Masquer un commentaire (POST /api/admin/comments/{id}/hide).</summary>
    [HttpPost("comments/{id:guid}/hide")]
    public async Task<IActionResult> HideComment(Guid id, CancellationToken cancellationToken)
        => await _sender.Send(new HideCommentCommand(id), cancellationToken) == HideResult.NotFound
            ? NotFound()
            : NoContent();
}
