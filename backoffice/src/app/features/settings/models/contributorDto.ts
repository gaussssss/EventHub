/** Contributeur de la page « À propos » (GET/POST/PATCH /api/admin/contributors). */
export interface ContributorDto {
  id: string;
  name: string;
  role: string;
  avatarUrl?: string | null;
  sortOrder: number;
}

/** Payload create/update d'un contributeur. */
export interface ContributorRequest {
  name: string;
  role: string;
  avatarUrl?: string | null;
  sortOrder: number;
}
