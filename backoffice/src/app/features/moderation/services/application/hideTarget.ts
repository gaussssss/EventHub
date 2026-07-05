import { inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ModerationService } from '../infrastructure/repository/moderation';
import { ModerationStates } from '../infrastructure/states/moderationStates';

/** Cas d'usage : masquer la cible d'un signalement (publication ou commentaire). */
@Injectable({ providedIn: 'root' })
export class HideTarget {
  /** Id du signalement en cours de traitement (désactive sa ligne / le bouton). */
  public readonly hidingId = signal<string | null>(null);

  private readonly repo = inject(ModerationService);
  private readonly states = inject(ModerationStates);
  private readonly toasts = inject(ToastStates);

  handler(reportId: string, targetType: string, targetId: string, callback?: () => void): void {
    const isComment = targetType.toLowerCase() === 'comment';
    const request: Observable<void> = isComment
      ? this.repo.hideComment(targetId)
      : this.repo.hidePost(targetId);

    this.hidingId.set(reportId);
    request.subscribe({
      next: () => {
        this.hidingId.set(null);
        this.states.removeById(reportId);
        this.toasts.success(isComment ? 'Commentaire masqué' : 'Publication masquée');
        callback?.();
      },
      error: () => {
        this.hidingId.set(null);
        this.toasts.error('Échec du masquage');
      },
    });
  }
}
