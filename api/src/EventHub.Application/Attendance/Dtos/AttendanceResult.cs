namespace EventHub.Application.Attendance;

public enum AttendanceResultStatus
{
    Ok,
    ActivityNotFound
}

public sealed record AttendanceResult(AttendanceResultStatus Status, int Credited);
