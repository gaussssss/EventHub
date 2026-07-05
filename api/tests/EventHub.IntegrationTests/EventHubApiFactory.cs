using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.IntegrationTests;

/// <summary>
/// Fabrique de test : remplace la base SQLite fichier par une base
/// <c>:memory:</c> partagée (connexion maintenue ouverte le temps du test).
/// </summary>
public class EventHubApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        // Harnais hermétique : environnement « Testing » (≠ Development) → les
        // user-secrets du dev (Authority Entra) ne sont PAS chargés, donc
        // entraEnabled=false → schéma d'authentification « dev » (X-User-Id /
        // X-User-Roles) sur lequel s'appuient les tests. Belt-and-suspenders :
        // on force aussi la config Entra à vide.
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Authority"] = string.Empty,
                ["Authentication:Audience"] = string.Empty,
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<EventHubDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<EventHubDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    /// <summary>Client authentifié « dev » : pose X-User-Id (+ rôles éventuels).</summary>
    public HttpClient CreateClientAs(Guid userId, params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        if (roles.Length > 0)
            client.DefaultRequestHeaders.Add("X-User-Roles", string.Join(',', roles));
        return client;
    }

    /// <summary>Client authentifié avec le rôle admin (back office).</summary>
    public HttpClient CreateAdminClient() => CreateClientAs(Guid.NewGuid(), "admin");

    /// <summary>Exécute une action sur un DbContext neuf (seed / assertions).</summary>
    public async Task WithDbAsync(Func<EventHubDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventHubDbContext>();
        await action(db);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
