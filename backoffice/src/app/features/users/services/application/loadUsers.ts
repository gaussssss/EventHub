import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { UserService } from '../infrastructure/repository/user';
import { UserStates } from '../infrastructure/states/userStates';

/** Cas d'usage : charger / rechercher la liste des utilisateurs. */
@Injectable({ providedIn: 'root' })
export class LoadUsers {
  public readonly isLoading = signal(false);
  public readonly query = signal('');

  private readonly repo = inject(UserService);
  private readonly states = inject(UserStates);
  private readonly toasts = inject(ToastStates);

  handler(q?: string): void {
    const term = q ?? this.query();
    this.query.set(term);
    this.isLoading.set(true);
    this.repo.search(term).subscribe({
      next: (users) => {
        this.isLoading.set(false);
        this.states.setUsers(users);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement des utilisateurs');
      },
    });
  }
}
