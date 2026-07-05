using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Registrations;

/// <summary>Commande « s'inscrire à une activité ».</summary>
public sealed record RegisterForActivityCommand(
    Guid UserId, Guid ActivityId, string? FormResponseId = null)
    : ICommand<RegistrationResult>;
