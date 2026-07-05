using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Users;

/// <summary>Requête « rechercher des utilisateurs » (back office).</summary>
public sealed record SearchUsersQuery(string? Query) : IQuery<IReadOnlyList<AdminUserDto>>;
