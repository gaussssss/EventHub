/** Organisateur (GET /api/admin/organizers). */
export interface OrganizerDto {
  id: string;
  name: string;
  contactEmail?: string | null;
}

/** Payload create/update (POST/PATCH /api/admin/organizers). */
export interface OrganizerRequest {
  name: string;
  contactEmail?: string | null;
}
