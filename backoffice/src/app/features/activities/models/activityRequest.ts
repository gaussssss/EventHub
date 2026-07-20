/** Payload create/update d'une activité (POST/PUT /api/admin/activities). */
export interface ActivityRequest {
  title: string;
  description: string;
  categoryId: string;
  organizerId?: string | null;
  startsAt: string;
  endsAt?: string | null;
  location: string;
  imageUrl: string;
  heartsReward: number;
  maxParticipants: number;
  participationCost: number;
  registrationUrl?: string | null;
  registrationDeadline?: string | null;
  isFeatured: boolean;
  status?: string | null;
}
