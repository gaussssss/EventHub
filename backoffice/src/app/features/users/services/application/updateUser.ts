import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { UpdateUserRequest } from '../../models/updateUserRequest';
import { UserService } from '../infrastructure/repository/user';
import { LoadUsers } from './loadUsers';

/** Cas d'usage : changer le rôle et/ou le statut d'un utilisateur. */
@Injectable({ providedIn: 'root' })
export class UpdateUser {
  /** Id en cours d'enregistrement (pour désactiver la ligne concernée). */
  public readonly savingId = signal<string | null>(null);

  private readonly repo = inject(UserService);
  private readonly toasts = inject(ToastStates);
  private readonly loadUsers = inject(LoadUsers);

  handler(id: string, request: UpdateUserRequest, callback?: () => void): void {
    this.savingId.set(id);
    this.repo.update(id, request).subscribe({
      next: () => {
        this.savingId.set(null);
        this.toasts.success('Utilisateur mis à jour');
        callback?.();
        this.loadUsers.handler();
      },
      error: () => {
        this.savingId.set(null);
        this.toasts.error('Échec de la mise à jour');
        this.loadUsers.handler(); // resynchronise l'affichage
      },
    });
  }
}
