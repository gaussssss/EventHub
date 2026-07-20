using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Attendance;

/// <summary>
/// Auto-émargement d'un étudiant via le QR de l'événement (POST
/// /api/activities/{id}/check-in). L'app scanne le QR (qui porte le
/// <c>Token</c> secret de l'activité) ; on confirme la présence de l'utilisateur
/// courant et on crédite les cœurs. Idempotent (pas de double crédit).
/// </summary>
public sealed record SelfCheckInCommand(Guid ActivityId, Guid UserId, Guid Token)
    : ICommand<SelfCheckInResult>;

public enum SelfCheckInStatus
{
    Ok,
    ActivityNotFound,
    InvalidToken,
    NotRegistered,
    OutsideWindow,
}

public sealed record SelfCheckInResult(
    SelfCheckInStatus Status, int HeartsAwarded, bool AlreadyCheckedIn);
