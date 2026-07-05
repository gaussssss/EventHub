import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { SettingsService } from '../infrastructure/repository/settings';
import { SettingsStates } from '../infrastructure/states/settingsStates';

/** Cas d'usage : charger les réglages de gamification. */
@Injectable({ providedIn: 'root' })
export class LoadGamification {
  public readonly isLoading = signal(false);

  private readonly repo = inject(SettingsService);
  private readonly states = inject(SettingsStates);
  private readonly toasts = inject(ToastStates);

  handler(): void {
    this.isLoading.set(true);
    this.repo.getGamification().subscribe({
      next: (settings) => {
        this.isLoading.set(false);
        this.states.setGamification(settings);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement des réglages');
      },
    });
  }
}
