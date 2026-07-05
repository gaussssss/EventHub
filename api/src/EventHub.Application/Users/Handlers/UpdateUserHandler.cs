using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Domain;

namespace EventHub.Application.Users;

/// <summary>Mise à jour d'un utilisateur au back office (PATCH /api/admin/users/{id}).</summary>
public sealed class UpdateUserHandler : ICommandHandler<UpdateUserCommand, UpdateUserStatus>
{
    private static readonly string[] AllowedStatuses = { "active", "suspended", "deleted" };

    private readonly IUserAdminService _users;

    public UpdateUserHandler(IUserAdminService users) => _users = users;

    public async Task<UpdateUserStatus> HandleAsync(
        UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var role = command.Role is null ? null : UserRoles.Normalize(command.Role);
        if (command.Role is not null && role is null)
            return UpdateUserStatus.InvalidRole;

        var status = command.Status?.ToLowerInvariant();
        if (status is not null && !AllowedStatuses.Contains(status))
            return UpdateUserStatus.InvalidStatus;

        if (!await _users.ExistsAsync(command.UserId, cancellationToken))
            return UpdateUserStatus.NotFound;

        if (role is not null)
            await _users.SetRoleAsync(command.UserId, role, cancellationToken);
        if (status is not null)
            await _users.SetStatusAsync(command.UserId, status, cancellationToken);

        return UpdateUserStatus.Updated;
    }
}
