using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

/// <summary>Publier une activité (POST /api/admin/activities/{id}/publish).</summary>
public sealed record PublishActivityCommand(Guid Id) : ICommand<ActivityActionStatus>;
