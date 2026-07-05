/** Payload PATCH /api/admin/users/{id} — rôle et/ou statut. */
export interface UpdateUserRequest {
  role?: string;
  status?: string;
}
