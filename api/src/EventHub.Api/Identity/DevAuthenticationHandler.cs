using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EventHub.Api.Identity;

/// <summary>
/// Schéma d'authentification de développement : authentifie via l'en-tête
/// <c>X-User-Id</c> (id interne) et, optionnellement, <c>X-User-Roles</c>
/// (rôles séparés par des virgules). Actif uniquement tant que l'auth Entra
/// n'est pas configurée — il permet à <c>[Authorize]</c> de fonctionner à
/// l'identique en dev/tests et en prod. À NE PAS activer en production réelle.
/// </summary>
public sealed class DevAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Dev";

    public DevAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-User-Id", out var rawUserId)
            || !Guid.TryParse(rawUserId, out var userId))
        {
            // Pas d'identité fournie → non authentifié (→ 401 sur [Authorize]).
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
        };

        if (Request.Headers.TryGetValue("X-User-Roles", out var rawRoles))
        {
            foreach (var role in rawRoles.ToString()
                         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
