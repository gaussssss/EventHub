using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

/// <summary>Toutes les inscriptions à plat pour l'export CSV.</summary>
public sealed record GetRegistrationsExportQuery : IQuery<IReadOnlyList<RegistrationExportRow>>;
