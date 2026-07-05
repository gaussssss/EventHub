namespace EventHub.Application.Social;

/// <summary>Issue d'une suppression de publication par son auteur.</summary>
public enum DeletePostStatus
{
    Deleted,
    NotFound,
    Forbidden,
}
