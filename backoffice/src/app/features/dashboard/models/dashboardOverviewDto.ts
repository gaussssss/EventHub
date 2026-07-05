/** KPIs du tableau de bord (GET /api/admin/dashboard/overview). */
export interface DashboardOverviewDto {
  totalUsers: number;
  totalActivities: number;
  publishedActivities: number;
  upcomingActivities: number;
  totalRegistrations: number;
  waitlistedRegistrations: number;
  totalHeartsAwarded: number;
  totalPosts: number;
}
