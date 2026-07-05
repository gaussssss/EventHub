using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

public sealed class GetRegistrationsExportHandler
    : IQueryHandler<GetRegistrationsExportQuery, IReadOnlyList<RegistrationExportRow>>
{
    private readonly IDashboardReadRepository _dashboard;

    public GetRegistrationsExportHandler(IDashboardReadRepository dashboard) =>
        _dashboard = dashboard;

    public Task<IReadOnlyList<RegistrationExportRow>> HandleAsync(
        GetRegistrationsExportQuery query, CancellationToken cancellationToken = default) =>
        _dashboard.GetRegistrationsForExportAsync(cancellationToken);
}
