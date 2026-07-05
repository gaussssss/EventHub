/** Payload d'une diffusion (POST /api/admin/notifications/broadcast). */
export interface BroadcastRequest {
  audience: string;
  title: string;
  body: string;
}
