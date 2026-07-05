using EventHub.Application.Common.Messaging;
using EventHub.Domain.ReadModels;

namespace EventHub.Application.Admin;

/// <summary>Détail complet d'une activité pour l'édition (back office).</summary>
public sealed record GetAdminActivityQuery(Guid Id) : IQuery<AdminActivityDetailDto?>;
