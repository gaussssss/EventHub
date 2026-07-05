using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Uploads;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly ISender _sender;

    public UploadsController(ISender sender) => _sender = sender;

    public sealed record SignBody(string Type, string ContentType);

    /// <summary>Obtenir une URL d'upload pré-signée (POST /api/uploads/sign).</summary>
    [HttpPost("sign")]
    public async Task<ActionResult<UploadTicket>> Sign(
        [FromBody] SignBody body, CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new SignUploadQuery(body.Type, body.ContentType), cancellationToken));
}
