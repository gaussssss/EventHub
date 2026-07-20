using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Contributors;

/// <summary>Liste des contributeurs (GET /api/about/contributors, tri par ordre).</summary>
public sealed record GetContributorsQuery : IQuery<IReadOnlyList<ContributorDto>>;
