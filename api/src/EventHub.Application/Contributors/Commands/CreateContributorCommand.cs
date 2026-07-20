using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Contributors;

/// <summary>Créer un contributeur (POST /api/admin/contributors).</summary>
public sealed record CreateContributorCommand(
    string Name, string Role, string? AvatarUrl, int SortOrder)
    : ICommand<ContributorMutationResult>;
