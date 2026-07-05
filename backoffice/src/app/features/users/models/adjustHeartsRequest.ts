/** Payload POST /api/admin/users/{id}/hearts — ajustement manuel (peut être négatif). */
export interface AdjustHeartsRequest {
  hearts: number;
  reason?: string;
}

/** Réponse : nouveau total de cœurs. */
export interface AdjustHeartsResult {
  totalHearts: number;
}
