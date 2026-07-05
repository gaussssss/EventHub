import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { OrganizerService } from '../infrastructure/repository/organizer';
import { OrganizerStates } from '../infrastructure/states/organizerStates';

/** Cas d'usage : charger la liste des organisateurs. */
@Injectable({ providedIn: 'root' })
export class LoadOrganizers {
  public readonly isLoading = signal(false);

  private readonly repo = inject(OrganizerService);
  private readonly states = inject(OrganizerStates);
  private readonly toasts = inject(ToastStates);

  handler(): void {
    this.isLoading.set(true);
    this.repo.getAll().subscribe({
      next: (organizers) => {
        this.isLoading.set(false);
        this.states.setOrganizers(organizers);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement des organisateurs');
      },
    });
  }
}
