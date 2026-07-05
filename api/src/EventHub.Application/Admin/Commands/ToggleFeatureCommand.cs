using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

/// <summary>Basculer la mise « à la une » (POST /api/admin/activities/{id}/feature).</summary>
public sealed record ToggleFeatureCommand(Guid Id) : ICommand<ToggleFeatureResult>;
