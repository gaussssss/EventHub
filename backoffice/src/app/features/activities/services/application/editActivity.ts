import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ActivityDetailDto } from '../../models/activityDetailDto';
import { ActivityService } from '../infrastructure/repository/activity';

/** Cas d'usage : charger le détail d'une activité pour l'éditer. */
@Injectable({ providedIn: 'root' })
export class EditActivity {
  public readonly detail = signal<ActivityDetailDto | null>(null);
  public readonly isLoading = signal(false);

  private readonly repo = inject(ActivityService);
  private readonly toasts = inject(ToastStates);

  load(id: string): void {
    this.isLoading.set(true);
    this.detail.set(null);
    this.repo.getById(id).subscribe({
      next: (detail) => {
        this.isLoading.set(false);
        this.detail.set(detail);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error("Erreur de chargement de l'activité");
      },
    });
  }

  clear(): void {
    this.detail.set(null);
  }
}
