/** Ligne de la liste d'activités au back office (GET /api/admin/activities). */
export interface AdminActivityDto {
  id: string;
  title: string;
  category: string;
  startsAt: string;
  location: string;
  /** 'draft' | 'published' | 'cancelled' | 'archived' */
  status: string;
  isFeatured: boolean;
  maxParticipants: number;
  currentParticipants: number;
}
