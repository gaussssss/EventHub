import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ModerationService } from '../infrastructure/repository/moderation';
import { ModerationStates } from '../infrastructure/states/moderationStates';

/** Cas d'usage : charger la file des signalements ouverts. */
@Injectable({ providedIn: 'root' })
export class LoadReports {
  public readonly isLoading = signal(false);

  private readonly repo = inject(ModerationService);
  private readonly states = inject(ModerationStates);
  private readonly toasts = inject(ToastStates);

  handler(): void {
    this.isLoading.set(true);
    this.repo.getReports().subscribe({
      next: (reports) => {
        this.isLoading.set(false);
        this.states.setReports(reports);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement des signalements');
      },
    });
  }
}
