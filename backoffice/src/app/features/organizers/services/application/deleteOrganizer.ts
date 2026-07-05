import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { OrganizerService } from '../infrastructure/repository/organizer';
import { LoadOrganizers } from './loadOrganizers';

/** Cas d'usage : supprimer un organisateur. */
@Injectable({ providedIn: 'root' })
export class DeleteOrganizer {
  public readonly deletingId = signal<string | null>(null);

  private readonly repo = inject(OrganizerService);
  private readonly toasts = inject(ToastStates);
  private readonly loadOrganizers = inject(LoadOrganizers);

  handler(id: string): void {
    this.deletingId.set(id);
    this.repo.delete(id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toasts.success('Organisateur supprimé');
        this.loadOrganizers.handler();
      },
      error: () => {
        this.deletingId.set(null);
        this.toasts.error('Échec de la suppression');
      },
    });
  }
}
