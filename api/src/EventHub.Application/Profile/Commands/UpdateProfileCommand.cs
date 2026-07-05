using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Profile;

/// <summary>Met à jour le profil de l'utilisateur courant (PATCH /api/me).</summary>
public sealed record UpdateProfileCommand(Guid UserId, string Name) : ICommand<bool>;
