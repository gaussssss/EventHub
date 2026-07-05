namespace EventHub.Domain.ReadModels;

/// <summary>
/// Statistiques d'une activité pour le back office
/// (GET /api/admin/dashboard/activities/{id}) : remplissage, présence, no-show.
/// Les taux sont des ratios 0..1 arrondis à 2 décimales.
/// </summary>
public sealed record ActivityDashboardDto
{
    public required Guid ActivityId { get; init; }
    public required string Title { get; init; }
    public int MaxParticipants { get; init; }
    public int Registered { get; init; }
    public int Attended { get; init; }
    public int Waitlisted { get; init; }
    public int NoShow { get; init; }
    public int Cancelled { get; init; }
    public double FillRate { get; init; }
    public double AttendanceRate { get; init; }
    public double NoShowRate { get; init; }
}
