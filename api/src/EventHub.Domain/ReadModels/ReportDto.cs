namespace EventHub.Domain.ReadModels;

/// <summary>Élément de la file de signalements affichée au back office.</summary>
public sealed record ReportDto(
    Guid Id,
    string TargetType,
    Guid TargetId,
    string Reason,
    string Status,
    string ReporterName,
    DateTime CreatedAt,
    // Aperçu du contenu signalé (pour que le modérateur le voie avant d'agir).
    string? TargetPreview = null,
    string? TargetImageUrl = null,
    string? TargetAuthorName = null);
