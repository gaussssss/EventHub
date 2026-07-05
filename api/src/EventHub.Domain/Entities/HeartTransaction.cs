using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Écriture (immuable) du grand livre des cœurs. Le total d'un utilisateur est la
/// somme de ses transactions (gains de présence, bonus, ajustements admin).
/// </summary>
public class HeartTransaction : BaseEntity
{
    public const string ReasonAttendance = "attendance";
    public const string ReasonAdjustment = "admin_adjust";

    private HeartTransaction() { } // EF Core

    public Guid UserId { get; private set; }
    public Guid? ActivityId { get; private set; }

    /// <summary>Snapshot du titre (historique lisible même si l'activité change).</summary>
    public string? ActivityTitle { get; private set; }

    public int Hearts { get; private set; }

    /// <summary>"attendance" | "bonus" | "admin_adjust".</summary>
    public string Reason { get; private set; } = null!;

    /// <summary>Crédit de cœurs à la présence confirmée (montant strictement positif).</summary>
    public static HeartTransaction ForAttendance(
        Guid userId, Guid activityId, string activityTitle, int hearts, DateTime nowUtc)
    {
        var tx = new HeartTransaction
        {
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            ActivityId = Guard.AgainstEmpty(activityId, nameof(activityId)),
            ActivityTitle = activityTitle,
            Hearts = Guard.AgainstNonPositive(hearts, nameof(hearts)),
            Reason = ReasonAttendance,
        };
        tx.MarkCreated(nowUtc);
        return tx;
    }

    /// <summary>Ajustement manuel par un admin (montant non nul, positif ou négatif).</summary>
    public static HeartTransaction ForAdjustment(
        Guid userId, int hearts, string reason, DateTime nowUtc)
    {
        if (hearts == 0)
            throw new DomainException("Un ajustement de cœurs ne peut pas être nul.");

        var tx = new HeartTransaction
        {
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            Hearts = hearts,
            Reason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason)),
        };
        tx.MarkCreated(nowUtc);
        return tx;
    }
}
