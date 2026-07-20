using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Contributors;

/// <summary>Mettre à jour un contributeur (PATCH /api/admin/contributors/{id}).</summary>
public sealed record UpdateContributorCommand(
    Guid Id, string Name, string Role, string? AvatarUrl, int SortOrder)
    : ICommand<CrudOutcome>;
