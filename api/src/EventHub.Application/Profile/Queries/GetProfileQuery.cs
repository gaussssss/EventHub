using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Profile;

/// <summary>Requête « profil de l'utilisateur courant ».</summary>
public sealed record GetProfileQuery(Guid UserId) : IQuery<ProfileDto?>;
