using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Contributors;

/// <summary>Supprimer un contributeur (DELETE /api/admin/contributors/{id}).</summary>
public sealed record DeleteContributorCommand(Guid Id) : ICommand<CrudOutcome>;
