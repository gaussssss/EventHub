using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Moderation;

/// <summary>File des signalements ouverts pour le back office (GET /api/admin/reports).</summary>
public sealed class GetOpenReportsHandler
    : IQueryHandler<GetOpenReportsQuery, IReadOnlyList<ReportDto>>
{
    private readonly IReportReadRepository _reports;

    public GetOpenReportsHandler(IReportReadRepository reports) => _reports = reports;

    public Task<IReadOnlyList<ReportDto>> HandleAsync(
        GetOpenReportsQuery query, CancellationToken cancellationToken = default) =>
        _reports.GetOpenAsync(cancellationToken);
}
