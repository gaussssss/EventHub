using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Uploads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IWebHostEnvironment _env;

    public UploadsController(ISender sender, IWebHostEnvironment env)
    {
        _sender = sender;
        _env = env;
    }

    public sealed record SignBody(string Type, string ContentType);

    /// <summary>Obtenir une URL d'upload pré-signée (POST /api/uploads/sign).</summary>
    [HttpPost("sign")]
    public async Task<ActionResult<UploadTicket>> Sign(
        [FromBody] SignBody body, CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new SignUploadQuery(body.Type, body.ContentType), cancellationToken));

    /// <summary>
    /// Upload direct d'une image (multipart « file »), stockée en local sous
    /// <c>wwwroot/uploads</c> et servie en statique. Renvoie l'URL absolue.
    /// Stockage de développement — à remplacer par un vrai blob store en prod.
    /// </summary>
    [Authorize]
    [HttpPost("image")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadImage(
        IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "fichier requis" });
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "type de fichier non supporté (image attendue)" });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || ext.Length > 5) ext = ".jpg";

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        await using (var stream = System.IO.File.Create(Path.Combine(uploadsDir, fileName)))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        return Ok(new { url });
    }
}
