using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Users;

/// <summary>Rôle et/ou statut à appliquer ; <c>null</c> = inchangé.</summary>
public sealed record UpdateUserCommand(Guid UserId, string? Role, string? Status)
    : ICommand<UpdateUserStatus>;
