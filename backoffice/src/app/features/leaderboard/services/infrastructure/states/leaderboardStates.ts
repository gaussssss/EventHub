import { Injectable, signal } from '@angular/core';
import { LeaderboardRow } from '../../../models/leaderboardRow';

/** Store signal du classement. */
@Injectable({ providedIn: 'root' })
export class LeaderboardStates {
  private readonly _rows = signal<LeaderboardRow[]>([]);
  readonly rows = this._rows.asReadonly();

  setRows(rows: LeaderboardRow[]): void {
    this._rows.set(rows);
  }

  reset(): void {
    this._rows.set([]);
  }
}
