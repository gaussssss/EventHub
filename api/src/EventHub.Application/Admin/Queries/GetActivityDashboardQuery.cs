using EventHub.Application.Common.Messaging;
using EventHub.Domain.ReadModels;

namespace EventHub.Application.Admin;

/// <summary>Statistiques d'une activité (remplissage, présence, no-show).</summary>
public sealed record GetActivityDashboardQuery(Guid ActivityId) : IQuery<ActivityDashboardDto?>;
