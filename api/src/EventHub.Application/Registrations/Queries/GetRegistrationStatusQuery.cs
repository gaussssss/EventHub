using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Registrations;

/// <summary>Statut d'inscription de l'utilisateur à une activité.</summary>
public sealed record GetRegistrationStatusQuery(Guid UserId, Guid ActivityId)
    : IQuery<RegistrationStatusDto>;
