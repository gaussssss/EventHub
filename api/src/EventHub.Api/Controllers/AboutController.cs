using EventHub.Application.Common.Messaging;
using EventHub.Application.Contributors;
using EventHub.Domain.ReadModels;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

/// <summary>Page « À propos » de l'app : liste publique des contributeurs.</summary>
[ApiController]
[Route("api/about")]
public class AboutController : ControllerBase
{
    private readonly ISender _sender;

    public AboutController(ISender sender) => _sender = sender;

    /// <summary>Contributeurs, triés par ordre (GET /api/about/contributors).</summary>
    [HttpGet("contributors")]
    public async Task<ActionResult<IReadOnlyList<ContributorDto>>> Contributors(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetContributorsQuery(), cancellationToken));
}
