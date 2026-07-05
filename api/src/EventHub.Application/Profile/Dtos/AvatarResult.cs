namespace EventHub.Application.Profile;

/// <summary>Résultat de la mise à jour d'avatar (statut + URL publique).</summary>
public sealed record AvatarResult(bool Updated, string? AvatarUrl);
