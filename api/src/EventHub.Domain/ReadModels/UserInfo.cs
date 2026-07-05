namespace EventHub.Domain.ReadModels;

/// <summary>Identité minimale d'un utilisateur (projection lecture).</summary>
public sealed record UserInfo(Guid Id, string? Name, string? Email, string? AvatarUrl);
