using EventHub.Domain.Common;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Gamification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/settings")]
public class AdminSettingsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminSettingsController(ISender sender) => _sender = sender;

    public sealed record GamificationBody(
        int SilverThreshold, int GoldThreshold, int DefaultAttendanceReward);

    /// <summary>Lire les réglages de gamification (GET /api/admin/settings/gamification).</summary>
    [HttpGet("gamification")]
    public async Task<ActionResult<GamificationSettingsDto>> GetGamification(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetGamificationSettingsQuery(), cancellationToken));

    /// <summary>Mettre à jour les réglages (PATCH /api/admin/settings/gamification).</summary>
    [HttpPatch("gamification")]
    public async Task<IActionResult> UpdateGamification(
        [FromBody] GamificationBody body, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sender.Send(
                new UpdateGamificationSettingsCommand(
                    body.SilverThreshold, body.GoldThreshold, body.DefaultAttendanceReward),
                cancellationToken);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
