using EventHub.Domain.Services;
using System.Security.Claims;

namespace EventHub.Api.Identity;

/// <summary>
/// Résout l'utilisateur courant sous forme de <b>Guid interne</b> : en mode Entra,
/// via le provisioning JIT (oid → user, exposé dans <c>HttpContext.Items</c>) ;
/// en dev, via le claim posé par le handler « Dev » ; en dernier recours,
/// l'en-tête <c>X-User-Id</c>.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? UserId
    {
        get
        {
            var http = _accessor.HttpContext;
            if (http is null) return null;

            // 1) Mode Entra : id interne résolu par le provisioning JIT (oid → user).
            if (http.Items.TryGetValue("InternalUserId", out var provisioned)
                && provisioned is Guid internalId)
                return internalId;

            // 2) Mode dev : le handler « Dev » place notre id interne dans NameIdentifier.
            var claim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var id))
                return id;

            // 3) Repli dev sans schéma d'auth (tests bas niveau).
            if (http.Request.Headers.TryGetValue("X-User-Id", out var header)
                && Guid.TryParse(header, out var headerId))
                return headerId;

            return null;
        }
    }
}
