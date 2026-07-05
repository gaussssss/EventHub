import { Injectable, signal } from '@angular/core';
import { AdminActivityDto } from '../../../models/adminActivityDto';

/** Store signal de la liste d'activités (back office). */
@Injectable({ providedIn: 'root' })
export class ActivityStates {
  private readonly _activities = signal<AdminActivityDto[]>([]);
  readonly activities = this._activities.asReadonly();

  setActivities(activities: AdminActivityDto[]): void {
    this._activities.set(activities);
  }

  reset(): void {
    this._activities.set([]);
  }
}
