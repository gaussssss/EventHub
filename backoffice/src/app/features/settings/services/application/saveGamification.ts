import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { httpErrorMessage } from '../../../../core/http/httpError';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { GamificationRequest } from '../../models/gamificationSettingsDto';
import { SettingsService } from '../infrastructure/repository/settings';
import { SettingsStates } from '../infrastructure/states/settingsStates';

/** Cas d'usage : enregistrer les réglages de gamification. */
@Injectable({ providedIn: 'root' })
export class SaveGamification {
  public readonly isSaving = signal(false);

  private readonly repo = inject(SettingsService);
  private readonly states = inject(SettingsStates);
  private readonly toasts = inject(ToastStates);

  handler(body: GamificationRequest): void {
    this.isSaving.set(true);
    this.repo.updateGamification(body).subscribe({
      next: (settings) => {
        this.isSaving.set(false);
        this.states.setGamification(settings);
        this.toasts.success('Réglages enregistrés');
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        // Surface le message métier (ex. « Or doit être supérieur à Argent »).
        this.toasts.error(httpErrorMessage(err) ?? "Échec de l'enregistrement");
      },
    });
  }
}
