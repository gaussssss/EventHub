using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Registrations;

/// <summary>Commande « annuler son inscription ».</summary>
public sealed record CancelRegistrationCommand(Guid UserId, Guid ActivityId)
    : ICommand<CancellationResult>;
