import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { httpErrorMessage } from '../../../../core/http/httpError';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { DevService } from '../infrastructure/repository/dev';

/** Cas d'usage (dev) : réinitialiser + régénérer les données de démonstration. */
@Injectable({ providedIn: 'root' })
export class SeedDevData {
  public readonly isSeeding = signal(false);

  private readonly repo = inject(DevService);
  private readonly toasts = inject(ToastStates);

  handler(callback?: () => void): void {
    this.isSeeding.set(true);
    this.repo.seed().subscribe({
      next: (r) => {
        this.isSeeding.set(false);
        this.toasts.success(
          `Seed OK : ${r.users} users · ${r.activities} activités · ${r.registrations} inscriptions · ${r.hearts} crédits · ${r.posts} posts · ${r.likes} likes · ${r.reports} signalements.`,
        );
        callback?.();
      },
      error: (err: HttpErrorResponse) => {
        this.isSeeding.set(false);
        this.toasts.error(httpErrorMessage(err) ?? 'Échec du seed');
      },
    });
  }
}
