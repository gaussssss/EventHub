import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ActivityService } from '../infrastructure/repository/activity';
import { RegistrationStates } from '../infrastructure/states/registrationStates';

/** Cas d'usage : charger les inscrits d'une activité. */
@Injectable({ providedIn: 'root' })
export class LoadRegistrations {
  public readonly isLoading = signal(false);

  private readonly repo = inject(ActivityService);
  private readonly states = inject(RegistrationStates);
  private readonly toasts = inject(ToastStates);

  handler(activityId: string): void {
    this.isLoading.set(true);
    this.repo.getRegistrations(activityId).subscribe({
      next: (rows) => {
        this.isLoading.set(false);
        this.states.setRows(rows);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement des inscrits');
      },
    });
  }
}
