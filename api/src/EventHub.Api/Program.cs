using System.Threading.RateLimiting;
using EventHub.Domain.Services;
using EventHub.Api.Health;
using EventHub.Api.Hubs;
using EventHub.Api.Identity;
using EventHub.Api.Middleware;
using EventHub.Api.Realtime;
using EventHub.Infrastructure;
using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Secrets/config : appsettings < user-secrets (dev) < variables d'environnement.
// Rien de sensible n'est stocké en dur (chaîne de connexion, Authority, stockage…).
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Services applicatifs portés par l'API (contexte HTTP, temps réel).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Gestion d'erreurs centralisée → ProblemDetails (RFC 7807).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Observabilité : journalisation des requêtes HTTP (méthode, chemin, statut, durée).
builder.Services.AddHttpLogging(options =>
    options.LoggingFields =
        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode |
        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.Duration);

// Santé : GET /health (connectivité base incluse).
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// CORS : origines autorisées via config (Cors:AllowedOrigins). Sans config → tout
// autoriser (dev). En prod, renseigner les origines réelles de l'app/back-office.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options => options.AddPolicy("Default", policy =>
{
    if (allowedOrigins is { Length: > 0 })
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    else
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));

// Limitation de débit : fenêtre fixe par IP (défaut 600 req/min), 429 si dépassé.
var permitPerMinute = builder.Configuration.GetValue("RateLimiting:PermitPerMinute", 600);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

// Authentification. Deux modes selon la config :
//  • Entra activé (Authority renseigné) → validation des jetons Entra + provisioning JIT.
//  • Sinon → schéma « Dev » (en-tête X-User-Id/X-User-Roles) pour que [Authorize]
//    fonctionne à l'identique en dev/tests. NE PAS utiliser le mode dev en prod.
var authority = builder.Configuration["Authentication:Authority"];
var entraEnabled = !string.IsNullOrWhiteSpace(authority);

var authBuilder = builder.Services.AddAuthentication(
    entraEnabled ? "Bearer" : DevAuthenticationHandler.SchemeName);

if (entraEnabled)
{
    authBuilder.AddJwtBearer("Bearer", options =>
    {
        options.Authority = authority;
        // Entra émet ses jetons v2 avec `aud` = client-id GUID nu, alors que
        // l'App ID URI est de la forme `api://<guid>`. On accepte donc les deux
        // (+ la valeur configurée telle quelle) pour éviter un 401 d'audience.
        var configuredAudience = builder.Configuration["Authentication:Audience"];
        if (!string.IsNullOrWhiteSpace(configuredAudience))
        {
            var guid = configuredAudience.StartsWith("api://", StringComparison.OrdinalIgnoreCase)
                ? configuredAudience["api://".Length..]
                : configuredAudience;
            options.TokenValidationParameters.ValidAudiences =
                new[] { configuredAudience, guid, $"api://{guid}" };
        }
        // Les rôles applicatifs Entra arrivent dans le claim « roles ».
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            // Restreint au tenant UQTR et au domaine @uqtr.ca (rejette invités/externes).
            OnTokenValidated = context =>
            {
                var expectedTenant = builder.Configuration["Authentication:TenantId"];
                var tid = context.Principal?.FindFirst("tid")?.Value;
                if (!string.IsNullOrEmpty(expectedTenant) && tid != expectedTenant)
                {
                    context.Fail("Tenant non autorisé.");
                    return Task.CompletedTask;
                }

                var allowedDomain = builder.Configuration["Authentication:AllowedEmailDomain"];
                if (!string.IsNullOrEmpty(allowedDomain))
                {
                    var email = context.Principal?.FindFirst("preferred_username")?.Value
                                ?? context.Principal?.FindFirst("email")?.Value;
                    if (email is null || !email.EndsWith("@" + allowedDomain, StringComparison.OrdinalIgnoreCase))
                        context.Fail("Domaine de courriel non autorisé.");
                }

                return Task.CompletedTask;
            },
            // Diagnostic bring-up : trace la cause précise d'un rejet de jeton
            // (expiration, audience, signature, issuer…) dans la console API.
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[Auth] ÉCHEC jeton : {context.Exception.GetType().Name} — {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                if (!string.IsNullOrEmpty(context.ErrorDescription))
                    Console.WriteLine($"[Auth] challenge 401 : {context.Error} — {context.ErrorDescription}");
                return Task.CompletedTask;
            },
        };
    });
}
else
{
    authBuilder.AddScheme<
        Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthenticationHandler>(
        DevAuthenticationHandler.SchemeName, _ => { });
}

builder.Services.AddAuthorization();

var app = builder.Build();

// Le gestionnaire d'exceptions doit envelopper le pipeline au plus tôt.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();
app.UseCors("Default");
app.UseRateLimiter();

app.UseAuthentication();
// Provisioning JIT (oid → utilisateur interne) juste après l'authentification Entra.
if (entraEnabled)
    app.UseMiddleware<EventHub.Api.Identity.EntraProvisioningMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

// GET /health → { status, version } (sonde de disponibilité).
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status == HealthStatus.Healthy ? "ok" : "degraded",
            version,
        });
    },
});

// Applique les migrations EF au démarrage (crée/mette à jour le schéma SQLite).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventHubDbContext>();
    db.Database.Migrate();

    // Seed des rôles applicatifs (idempotent). Indispensable au provisioning JIT
    // Entra (rôle « student » assigné au premier login) et aux [Authorize(Roles=…)]
    // des contrôleurs admin, sinon AddToRoleAsync lève « Role … does not exist ».
    var roleManager = scope.ServiceProvider
        .GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<
            Microsoft.AspNetCore.Identity.IdentityRole<Guid>>>();
    foreach (var role in new[] { "student", "organizer", "moderator", "admin" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole<Guid>(role));
    }
}

app.Run();

/// <summary>Exposé pour <c>WebApplicationFactory</c> (tests d'intégration).</summary>
public partial class Program;
