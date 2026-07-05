using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EventHub.Api.Health;

/// <summary>Vérifie la connectivité à la base (utilisé par GET /health).</summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly EventHubDbContext _db;

    public DatabaseHealthCheck(EventHubDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Base de données accessible.")
                : HealthCheckResult.Unhealthy("Base de données inaccessible.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Échec de la vérification de la base.", ex);
        }
    }
}
