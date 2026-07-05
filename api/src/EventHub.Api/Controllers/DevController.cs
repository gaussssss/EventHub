using EventHub.Infrastructure.Dev;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

/// <summary>
/// Outils de DÉVELOPPEMENT — disponibles uniquement en environnement Development
/// (404 sinon) et réservés au rôle admin. Ne fait rien en production.
/// </summary>
[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/dev")]
public class DevController : ControllerBase
{
    private readonly DevDataSeeder _seeder;
    private readonly IWebHostEnvironment _env;

    public DevController(DevDataSeeder seeder, IWebHostEnvironment env)
    {
        _seeder = seeder;
        _env = env;
    }

    /// <summary>
    /// Réinitialise puis régénère un jeu de données de démo (POST /api/admin/dev/seed).
    /// Efface uniquement les données de seed, préserve les vrais utilisateurs du tenant.
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var result = await _seeder.ResetAndSeedAsync(cancellationToken);
        return Ok(result);
    }
}
