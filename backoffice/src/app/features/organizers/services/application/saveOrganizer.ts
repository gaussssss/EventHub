import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { httpErrorMessage } from '../../../../core/http/httpError';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { OrganizerRequest } from '../../models/organizerDto';
import { OrganizerService } from '../infrastructure/repository/organizer';
import { LoadOrganizers } from './loadOrganizers';

/** Cas d'usage : créer (id null) ou mettre à jour un organisateur. */
@Injectable({ providedIn: 'root' })
export class SaveOrganizer {
  public readonly isSaving = signal(false);

  private readonly repo = inject(OrganizerService);
  private readonly toasts = inject(ToastStates);
  private readonly loadOrganizers = inject(LoadOrganizers);

  handler(id: string | null, request: OrganizerRequest, callback?: () => void): void {
    if (!request.name.trim()) {
      this.toasts.warning('Le nom est requis.');
      return;
    }

    const call: Observable<unknown> = id
      ? this.repo.update(id, request)
      : this.repo.create(request);

    this.isSaving.set(true);
    call.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.toasts.success(id ? 'Organisateur mis à jour' : 'Organisateur créé');
        callback?.();
        this.loadOrganizers.handler();
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.toasts.error(httpErrorMessage(err) ?? "Échec de l'enregistrement");
      },
    });
  }
}
