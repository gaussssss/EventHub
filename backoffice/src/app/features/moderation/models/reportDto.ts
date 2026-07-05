/** Signalement affiché dans la file de modération (GET /api/admin/reports). */
export interface ReportDto {
  id: string;
  /** 'post' | 'comment' (cible du signalement). */
  targetType: string;
  targetId: string;
  reason: string;
  status: string;
  reporterName: string;
  createdAt: string;
  /** Aperçu du contenu signalé (caption de post ou texte de commentaire). */
  targetPreview?: string | null;
  /** Image du post signalé (le cas échéant). */
  targetImageUrl?: string | null;
  /** Auteur du contenu signalé. */
  targetAuthorName?: string | null;
}
