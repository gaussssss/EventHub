/** Statistiques d'une activité (GET /api/admin/dashboard/activities/{id}). Taux 0..1. */
export interface ActivityDashboardDto {
  activityId: string;
  title: string;
  maxParticipants: number;
  registered: number;
  attended: number;
  waitlisted: number;
  noShow: number;
  cancelled: number;
  fillRate: number;
  attendanceRate: number;
  noShowRate: number;
}
