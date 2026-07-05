using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Attendance;

/// <summary>Commande « marquer les présences » (back office).</summary>
public sealed record MarkAttendanceCommand(Guid ActivityId, IReadOnlyCollection<Guid> UserIds)
    : ICommand<AttendanceResult>;
