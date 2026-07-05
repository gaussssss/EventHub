using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Users;
using EventHub.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

/// <summary>
/// Annuaire des utilisateurs pour le back office : joint le rôle Identity et
/// somme les cœurs. Recherche filtrée sur le nom ou le courriel.
/// </summary>
public sealed class UserAdminReadRepository : IUserAdminReadRepository
{
    private readonly EventHubDbContext _db;

    public UserAdminReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<AdminUserDto>> SearchAsync(
        string? query, CancellationToken cancellationToken = default)
    {
        var users = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            users = users.Where(u =>
                (u.Name != null && EF.Functions.Like(u.Name, $"%{q}%"))
                || (u.Email != null && EF.Functions.Like(u.Email, $"%{q}%")));
        }

        return await users
            .OrderBy(u => u.Name)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Name ?? string.Empty,
                u.Email ?? string.Empty,
                (from ur in _db.UserRoles
                 join r in _db.Roles on ur.RoleId equals r.Id
                 where ur.UserId == u.Id
                 select r.Name).FirstOrDefault() ?? UserRoles.Student,
                u.Status,
                _db.HeartTransactions.Where(h => h.UserId == u.Id).Sum(h => h.Hearts)))
            .ToListAsync(cancellationToken);
    }
}
