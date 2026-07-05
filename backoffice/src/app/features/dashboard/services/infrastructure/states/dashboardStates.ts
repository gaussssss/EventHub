import { Injectable, signal } from '@angular/core';
import { DashboardOverviewDto } from '../../../models/dashboardOverviewDto';

/** Store signal des KPIs du tableau de bord. */
@Injectable({ providedIn: 'root' })
export class DashboardStates {
  private readonly _overview = signal<DashboardOverviewDto | null>(null);
  readonly overview = this._overview.asReadonly();

  setOverview(overview: DashboardOverviewDto): void {
    this._overview.set(overview);
  }

  reset(): void {
    this._overview.set(null);
  }
}
