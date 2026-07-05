using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Users;

/// <summary>Commande « ajuster manuellement les cœurs d'un utilisateur » (admin).</summary>
public sealed record AdjustHeartsCommand(Guid UserId, int Hearts, string Reason)
    : ICommand<AdjustHeartsResult>;
