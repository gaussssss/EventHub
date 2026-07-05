/** Compte-rendu du seed de développement (POST /api/admin/dev/seed). */
export interface SeedResult {
  users: number;
  categories: number;
  organizers: number;
  activities: number;
  registrations: number;
  hearts: number;
  posts: number;
  comments: number;
  likes: number;
  reports: number;
}
