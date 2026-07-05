namespace EventHub.Domain.ReadModels;

/// <summary>KPIs du tableau de bord back office (GET /api/admin/dashboard/overview).</summary>
public sealed record DashboardOverviewDto
{
    public int TotalUsers { get; init; }
    public int TotalActivities { get; init; }
    public int PublishedActivities { get; init; }
    public int UpcomingActivities { get; init; }
    public int TotalRegistrations { get; init; }
    public int WaitlistedRegistrations { get; init; }
    public long TotalHeartsAwarded { get; init; }
    public int TotalPosts { get; init; }
}
