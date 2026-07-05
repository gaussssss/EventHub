import { inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ActivityService } from '../infrastructure/repository/activity';
import { LoadActivities } from './loadActivities';

/** Cas d'usage : actions sur une activité (publier, annuler, mettre à la une). */
@Injectable({ providedIn: 'root' })
export class ActivityActions {
  /** Id de l'activité en cours d'action (désactive sa ligne). */
  public readonly busyId = signal<string | null>(null);

  private readonly repo = inject(ActivityService);
  private readonly toasts = inject(ToastStates);
  private readonly loadActivities = inject(LoadActivities);

  publish(id: string): void {
    this.run(id, this.repo.publish(id), 'Activité publiée');
  }

  cancel(id: string): void {
    this.run(id, this.repo.cancel(id), 'Activité annulée');
  }

  toggleFeature(id: string): void {
    this.busyId.set(id);
    this.repo.feature(id).subscribe({
      next: (res) => {
        this.busyId.set(null);
        this.toasts.success(res.isFeatured ? 'Mise à la une' : 'Retirée de la une');
        this.loadActivities.handler();
      },
      error: () => {
        this.busyId.set(null);
        this.toasts.error("Échec de l'action");
      },
    });
  }

  private run(id: string, call: Observable<void>, successMessage: string): void {
    this.busyId.set(id);
    call.subscribe({
      next: () => {
        this.busyId.set(null);
        this.toasts.success(successMessage);
        this.loadActivities.handler();
      },
      error: () => {
        this.busyId.set(null);
        this.toasts.error("Échec de l'action");
      },
    });
  }
}
