namespace EventHub.Domain.ReadModels;

/// <summary>Ligne de la liste des utilisateurs au back office.</summary>
public sealed record AdminUserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string Status,
    int TotalHearts);
