using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Profile;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly EventHubDbContext _db;

    public UserReadRepository(EventHubDbContext db) => _db = db;

    public Task<UserInfo?> GetAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserInfo(u.Id, u.Name, u.Email, u.AvatarUrl))
            .FirstOrDefaultAsync(cancellationToken);
}
