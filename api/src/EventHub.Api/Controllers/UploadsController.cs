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
    /// <c>wwwroot/uploads</c> et servie en statique. Renvoie un **chemin relatif**
    /// (<c>/uploads/…</c>) : on ne persiste jamais le nom de domaine en base, pour
    /// que les fichiers restent valides si l'hôte change (dev → prod, etc.). Les
    /// clients résolvent ce chemin contre leur base API configurée.
    ///
    /// SÉCURITÉ : le type est déterminé par les **octets de signature** du fichier
    /// (magic bytes), jamais par le <c>Content-Type</c> ou l'extension fournis par
    /// le client. Seuls JPEG/PNG/WEBP/GIF sont acceptés ; l'extension stockée est
    /// dérivée du type réel. Empêche l'hébergement de HTML/SVG/JS actif servi
    /// depuis l'origine de l'API (XSS stocké).
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

        // Lecture des premiers octets pour détecter le vrai format (indépendant
        // du nom de fichier et du Content-Type, tous deux fournis par le client).
        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var read = await ReadExactAsync(input, header, cancellationToken);
        var ext = DetectImageExtension(header.AsSpan(0, read));
        if (ext is null)
            return BadRequest(new { error = "format non supporté (JPEG, PNG, WEBP ou GIF attendu)" });

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        await using (var output = System.IO.File.Create(Path.Combine(uploadsDir, fileName)))
        {
            // Réécrit l'en-tête déjà lu, puis le reste du flux.
            await output.WriteAsync(header.AsMemory(0, read), cancellationToken);
            await input.CopyToAsync(output, cancellationToken);
        }

        // Chemin relatif uniquement — jamais le domaine (portabilité dev/prod).
        var url = $"/uploads/{fileName}";
        return Ok(new { url });
    }

    /// <summary>Lit jusqu'à <paramref name="buffer"/>.Length octets (flux réseau).</summary>
    private static async Task<int> ReadExactAsync(
        Stream stream, byte[] buffer, CancellationToken ct)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(total), ct);
            if (n == 0) break;
            total += n;
        }
        return total;
    }

    /// <summary>
    /// Extension canonique (« .jpg »…) déduite des octets de signature, ou
    /// <c>null</c> si le format n'est pas une image bitmap autorisée.
    /// </summary>
    private static string? DetectImageExtension(ReadOnlySpan<byte> b)
    {
        // JPEG : FF D8 FF
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return ".jpg";
        // PNG : 89 50 4E 47 0D 0A 1A 0A
        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47 &&
            b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return ".png";
        // GIF : "GIF87a" / "GIF89a"
        if (b.Length >= 6 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x38 &&
            (b[4] == 0x37 || b[4] == 0x39) && b[5] == 0x61)
            return ".gif";
        // WEBP : "RIFF" .... "WEBP"
        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46 &&
            b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50)
            return ".webp";
        return null;
    }
}
