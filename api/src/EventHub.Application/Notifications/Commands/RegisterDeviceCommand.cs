using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Notifications;

/// <summary>Enregistrer un jeton push (POST /api/me/devices). Idempotent par jeton.</summary>
public sealed record RegisterDeviceCommand(Guid UserId, string PushToken, string? Platform)
    : ICommand<Guid>;
