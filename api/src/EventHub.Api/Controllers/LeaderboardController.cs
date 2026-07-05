using EventHub.Domain.ReadModels;
using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Leaderboard;
using EventHub.Application.Stats;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api")]
public class LeaderboardController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public LeaderboardController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    /// <summary>Classement global des cœurs (GET /api/leaderboard).</summary>
    [HttpGet("leaderboard")]
    public async Task<ActionResult<IReadOnlyList<LeaderboardRow>>> Leaderboard(
        [FromQuery] int page, CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new GetLeaderboardQuery(_currentUser.UserId, page < 1 ? 1 : page),
            cancellationToken));

    /// <summary>Badges communautaires (GET /api/stats/community).</summary>
    [HttpGet("stats/community")]
    public async Task<ActionResult<CommunityStatsDto>> CommunityStats(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetCommunityStatsQuery(), cancellationToken));
}
