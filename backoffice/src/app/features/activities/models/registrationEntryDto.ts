/** Inscrit d'une activité (GET /api/admin/activities/{id}/registrations). */
export interface RegistrationEntryDto {
  userId: string;
  name?: string | null;
  email?: string | null;
  /** 'registered' | 'attended' | 'waitlisted' | 'noshow' */
  status: string;
  registeredAt: string;
}
