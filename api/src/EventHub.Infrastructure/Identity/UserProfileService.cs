using EventHub.Domain.Services;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Infrastructure.Identity;

/// <summary>Écritures profil (nom, avatar) via ASP.NET Core Identity.</summary>
public sealed class UserProfileService : IUserProfileService
{
    private readonly UserManager<ApplicationUser> _users;

    public UserProfileService(UserManager<ApplicationUser> users) => _users = users;

    public async Task<bool> UpdateNameAsync(
        Guid userId, string name, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.Name = name;
        await _users.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UpdateAvatarAsync(
        Guid userId, string avatarUrl, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.AvatarUrl = avatarUrl;
        await _users.UpdateAsync(user);
        return true;
    }
}
