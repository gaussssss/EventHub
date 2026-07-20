import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { AdjustHeartsRequest } from '../../models/adjustHeartsRequest';
import { UserService } from '../infrastructure/repository/user';
import { LoadUsers } from './loadUsers';

/** Cas d'usage : ajuster manuellement les cœurs d'un utilisateur. */
@Injectable({ providedIn: 'root' })
export class AwardHearts {
  public readonly isSaving = signal(false);

  private readonly repo = inject(UserService);
  private readonly toasts = inject(ToastStates);
  private readonly loadUsers = inject(LoadUsers);

  handler(id: string, request: AdjustHeartsRequest, callback?: () => void): void {
    if (!request.hearts) {
      this.toasts.warning('Indiquez un nombre de cœurs non nul.');
      return;
    }

    this.isSaving.set(true);
    this.repo.adjustHearts(id, request).subscribe({
      next: (result) => {
        this.isSaving.set(false);
        this.toasts.success(`Cœurs ajustés, nouveau total : ${result.totalHearts}`);
        callback?.();
        this.loadUsers.handler();
      },
      error: () => {
        this.isSaving.set(false);
        this.toasts.error("Échec de l'ajustement des cœurs");
      },
    });
  }
}
