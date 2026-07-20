using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Profile;

/// <summary>
/// Met à jour l'avatar de l'utilisateur courant (POST /api/me/avatar). Le fichier
/// a déjà été uploadé (POST /api/uploads/image) ; on persiste ici le chemin
/// renvoyé (relatif, sans domaine — cf. UploadsController).
/// </summary>
public sealed record UpdateAvatarCommand(Guid UserId, string AvatarUrl) : ICommand<AvatarResult>;
