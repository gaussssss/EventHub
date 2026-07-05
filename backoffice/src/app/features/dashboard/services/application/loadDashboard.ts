import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { DashboardService } from '../infrastructure/repository/dashboard';
import { DashboardStates } from '../infrastructure/states/dashboardStates';

/** Cas d'usage : charger les KPIs du tableau de bord. */
@Injectable({ providedIn: 'root' })
export class LoadDashboard {
  public readonly isLoading = signal(false);

  private readonly repo = inject(DashboardService);
  private readonly states = inject(DashboardStates);
  private readonly toasts = inject(ToastStates);

  handler(): void {
    this.isLoading.set(true);
    this.repo.getOverview().subscribe({
      next: (overview) => {
        this.isLoading.set(false);
        this.states.setOverview(overview);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement du tableau de bord');
      },
    });
  }
}
