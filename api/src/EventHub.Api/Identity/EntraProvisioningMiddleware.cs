using System.Security.Claims;
using EventHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Api.Identity;

/// <summary>
/// Provisioning JIT (mode Entra) : à partir du claim <c>oid</c> du jeton validé,
/// retrouve l'utilisateur interne (ou le crée au premier login) et expose SON
/// Guid via <c>HttpContext.Items["InternalUserId"]</c>. C'est ce Guid interne —
/// et non l'<c>oid</c> Entra — qui identifie l'utilisateur dans tout le domaine.
///
/// L'<b>autorisation</b> repose sur les rôles stockés en base (et non sur des
/// app roles Entra) : ce middleware injecte les rôles DB de l'utilisateur comme
/// claims <c>roles</c> dans le principal, de sorte que <c>[Authorize(Roles=…)]</c>
/// fonctionne. Le premier admin est amorcé via <c>Authentication:AdminEmails</c>.
/// </summary>
public sealed class EntraProvisioningMiddleware
{
    private const string DefaultRole = "student";
    private const string AdminRole = "admin";
    private const string RoleClaimType = "roles";

    private readonly RequestDelegate _next;

    public EntraProvisioningMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context, UserManager<ApplicationUser> users, IConfiguration config)
    {
        var oid = context.User.FindFirst("oid")?.Value
                  ?? context.User.FindFirst(
                      "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        if (!string.IsNullOrEmpty(oid))
        {
            var user = await FindOrCreateAsync(context.User, oid, users);
            if (user is not null)
            {
                context.Items["InternalUserId"] = user.Id;
                await EnsureBootstrapAdminAsync(user, users, config);
                await InjectRoleClaimsAsync(context, user, users);
            }
        }

        await _next(context);
    }

    private static async Task<ApplicationUser?> FindOrCreateAsync(
        ClaimsPrincipal principal, string oid, UserManager<ApplicationUser> users)
    {
        var existing = users.Users.FirstOrDefault(u => u.EntraObjectId == oid);
        if (existing is not null)
            return existing;

        var email = principal.FindFirst("preferred_username")?.Value
                    ?? principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst("email")?.Value;
        var name = principal.FindFirst("name")?.Value
                   ?? principal.FindFirst(ClaimTypes.Name)?.Value;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            EntraObjectId = oid,
            UserName = email ?? oid,
            Email = email,
            Name = name,
        };

        var result = await users.CreateAsync(user);
        if (!result.Succeeded)
            return null;

        await users.AddToRoleAsync(user, DefaultRole);
        return user;
    }

    /// <summary>
    /// Amorçage : tout utilisateur dont le courriel figure dans
    /// <c>Authentication:AdminEmails</c> est (ré)assigné au seul rôle <c>admin</c>.
    /// Sert à désigner le premier admin sans passer par le back-office lui-même.
    /// </summary>
    private static async Task EnsureBootstrapAdminAsync(
        ApplicationUser user, UserManager<ApplicationUser> users, IConfiguration config)
    {
        var adminEmails = config.GetSection("Authentication:AdminEmails").Get<string[]>()
                          ?? Array.Empty<string>();
        if (adminEmails.Length == 0)
            return;

        var email = user.Email ?? user.UserName;
        if (email is null)
            return;

        var listed = adminEmails.Any(e => string.Equals(e, email, StringComparison.OrdinalIgnoreCase));
        if (!listed || await users.IsInRoleAsync(user, AdminRole))
            return;

        var current = await users.GetRolesAsync(user);
        if (current.Count > 0)
            await users.RemoveFromRolesAsync(user, current);
        await users.AddToRoleAsync(user, AdminRole);
    }

    /// <summary>
    /// Ajoute les rôles DB de l'utilisateur comme claims <c>roles</c> au principal
    /// courant (le <c>RoleClaimType</c> de JwtBearer est <c>roles</c>), pour que
    /// <c>[Authorize(Roles=…)]</c> s'appuie sur la base.
    /// </summary>
    private static async Task InjectRoleClaimsAsync(
        HttpContext context, ApplicationUser user, UserManager<ApplicationUser> users)
    {
        if (context.User.Identity is not ClaimsIdentity identity)
            return;

        foreach (var role in await users.GetRolesAsync(user))
        {
            if (!identity.HasClaim(RoleClaimType, role))
                identity.AddClaim(new Claim(RoleClaimType, role));
        }
    }
}
