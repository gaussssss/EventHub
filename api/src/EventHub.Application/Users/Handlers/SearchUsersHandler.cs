using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Users;

/// <summary>Recherche d'utilisateurs au back office (GET /api/admin/users?q=).</summary>
public sealed class SearchUsersHandler
    : IQueryHandler<SearchUsersQuery, IReadOnlyList<AdminUserDto>>
{
    private readonly IUserAdminReadRepository _users;

    public SearchUsersHandler(IUserAdminReadRepository users) => _users = users;

    public Task<IReadOnlyList<AdminUserDto>> HandleAsync(
        SearchUsersQuery query, CancellationToken cancellationToken = default) =>
        _users.SearchAsync(query.Query, cancellationToken);
}
