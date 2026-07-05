namespace EventHub.Application.Admin;

/// <summary>Résultat du toggle « à la une » (statut + nouvel état).</summary>
public sealed record ToggleFeatureResult(ActivityActionStatus Status, bool IsFeatured);
