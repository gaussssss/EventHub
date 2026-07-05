using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Activities;

/// <summary>Requête « détail d'une activité ».</summary>
public sealed record GetActivityByIdQuery(Guid Id) : IQuery<ActivityDto?>;
