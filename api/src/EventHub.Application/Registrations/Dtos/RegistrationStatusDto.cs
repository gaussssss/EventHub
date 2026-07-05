namespace EventHub.Application.Registrations;

/// <summary>
/// Statut d'inscription de l'utilisateur courant à une activité
/// (GET /api/activities/{id}/registration).
/// </summary>
public sealed record RegistrationStatusDto(
    bool IsRegistered,
    string? Status,
    DateTime? RegisteredAt);
