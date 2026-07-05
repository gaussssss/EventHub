import { inject, Injectable, signal } from '@angular/core';
import { ActivityDashboardDto } from '../../models/activityDashboardDto';
import { ActivityService } from '../infrastructure/repository/activity';

/** Cas d'usage : charger les statistiques d'une activité (remplissage, présence, no-show). */
@Injectable({ providedIn: 'root' })
export class LoadActivityDashboard {
  public readonly dashboard = signal<ActivityDashboardDto | null>(null);
  public readonly isLoading = signal(false);

  private readonly repo = inject(ActivityService);

  handler(activityId: string): void {
    this.isLoading.set(true);
    this.dashboard.set(null);
    this.repo.getDashboard(activityId).subscribe({
      next: (dto) => {
        this.isLoading.set(false);
        this.dashboard.set(dto);
      },
      error: () => {
        this.isLoading.set(false); // silencieux : le bandeau de stats est optionnel
      },
    });
  }
}
