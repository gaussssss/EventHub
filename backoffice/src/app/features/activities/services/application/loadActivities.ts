import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ActivityService } from '../infrastructure/repository/activity';
import { ActivityStates } from '../infrastructure/states/activityStates';

/** Cas d'usage : charger la liste des activités (tous statuts). */
@Injectable({ providedIn: 'root' })
export class LoadActivities {
  public readonly isLoading = signal(false);

  private readonly repo = inject(ActivityService);
  private readonly states = inject(ActivityStates);
  private readonly toasts = inject(ToastStates);

  handler(): void {
    this.isLoading.set(true);
    this.repo.getAll().subscribe({
      next: (activities) => {
        this.isLoading.set(false);
        this.states.setActivities(activities);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement des activités');
      },
    });
  }
}
