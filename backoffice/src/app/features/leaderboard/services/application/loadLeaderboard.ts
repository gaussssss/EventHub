import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { LeaderboardService } from '../infrastructure/repository/leaderboard';
import { LeaderboardStates } from '../infrastructure/states/leaderboardStates';

/** Cas d'usage : charger une page du classement. */
@Injectable({ providedIn: 'root' })
export class LoadLeaderboard {
  public readonly isLoading = signal(false);
  public readonly page = signal(1);

  private readonly repo = inject(LeaderboardService);
  private readonly states = inject(LeaderboardStates);
  private readonly toasts = inject(ToastStates);

  handler(page: number = this.page()): void {
    const target = page < 1 ? 1 : page;
    this.isLoading.set(true);
    this.repo.getLeaderboard(target).subscribe({
      next: (rows) => {
        this.isLoading.set(false);
        if (rows.length === 0 && target > 1) {
          this.toasts.info('Fin du classement.');
          return; // reste sur la page courante
        }
        this.page.set(target);
        this.states.setRows(rows);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement du classement');
      },
    });
  }
}
