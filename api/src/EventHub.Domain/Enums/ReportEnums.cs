namespace EventHub.Domain.Enums;

/// <summary>Type de contenu visé par un signalement.</summary>
public enum ReportTargetType
{
    Post,
    Comment
}

/// <summary>Cycle de vie d'un signalement de modération.</summary>
public enum ReportStatus
{
    Open,
    Resolved,
    Dismissed
}
