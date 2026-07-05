using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Profile;

/// <summary>
/// Met à jour l'avatar de l'utilisateur courant (POST /api/me/avatar). Le fichier
/// est stocké via le service de stockage ; on n'en garde que l'URL publique.
/// </summary>
public sealed record UpdateAvatarCommand(Guid UserId, string ContentType) : ICommand<AvatarResult>;
