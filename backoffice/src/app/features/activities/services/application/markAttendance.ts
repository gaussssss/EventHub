import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ActivityService } from '../infrastructure/repository/activity';
import { LoadRegistrations } from './loadRegistrations';

/** Cas d'usage : marquer les présences (crédite les cœurs). */
@Injectable({ providedIn: 'root' })
export class MarkAttendance {
  public readonly isSaving = signal(false);

  private readonly repo = inject(ActivityService);
  private readonly toasts = inject(ToastStates);
  private readonly loadRegistrations = inject(LoadRegistrations);

  handler(activityId: string, userIds: string[], callback?: () => void): void {
    if (userIds.length === 0) {
      this.toasts.warning('Sélectionnez au moins un inscrit.');
      return;
    }

    this.isSaving.set(true);
    this.repo.markAttendance(activityId, userIds).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.toasts.success(
          `${userIds.length} présence(s) enregistrée(s), ${res.credited} crédit(s) de cœurs.`,
        );
        callback?.();
        this.loadRegistrations.handler(activityId);
      },
      error: () => {
        this.isSaving.set(false);
        this.toasts.error("Échec de l'enregistrement des présences");
      },
    });
  }
}
