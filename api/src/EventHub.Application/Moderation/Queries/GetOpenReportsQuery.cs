using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Moderation;

/// <summary>Requête « file des signalements ouverts » (back office).</summary>
public sealed record GetOpenReportsQuery : IQuery<IReadOnlyList<ReportDto>>;
