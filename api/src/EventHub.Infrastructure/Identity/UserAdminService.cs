using EventHub.Domain.Services;
using EventHub.Application.Users;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Infrastructure.Identity;

/// <summary>
/// Gestion des rôles/statuts utilisateur via ASP.NET Core Identity. Un seul rôle
/// à la fois : <see cref="SetRoleAsync"/> remplace le rôle courant (et crée le
/// rôle Identity au besoin).
/// </summary>
public sealed class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole<Guid>> _roles;

    public UserAdminService(
        UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _users.FindByIdAsync(userId.ToString()) is not null;

    public async Task SetRoleAsync(
        Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return;

        if (!await _roles.RoleExistsAsync(role))
            await _roles.CreateAsync(new IdentityRole<Guid>(role));

        var current = await _users.GetRolesAsync(user);
        if (current.Count > 0)
            await _users.RemoveFromRolesAsync(user, current);
        await _users.AddToRoleAsync(user, role);
    }

    public async Task SetStatusAsync(
        Guid userId, string status, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return;

        user.Status = status;
        await _users.UpdateAsync(user);
    }
}
