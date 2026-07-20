/** Détail complet d'une activité pour l'édition (GET /api/admin/activities/{id}). */
export interface ActivityDetailDto {
  id: string;
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
  status: string;
  /** Jeton d'émargement (encodé dans le QR de présence). */
  checkInToken: string;
}
