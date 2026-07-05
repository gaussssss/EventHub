using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

/// <summary>Annuler une activité (POST /api/admin/activities/{id}/cancel).</summary>
public sealed record CancelActivityCommand(Guid Id) : ICommand<ActivityActionStatus>;
