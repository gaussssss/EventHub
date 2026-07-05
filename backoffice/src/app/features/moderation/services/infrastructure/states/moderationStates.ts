import { Injectable, signal } from '@angular/core';
import { ReportDto } from '../../../models/reportDto';

/** Store signal de la file de signalements. */
@Injectable({ providedIn: 'root' })
export class ModerationStates {
  private readonly _reports = signal<ReportDto[]>([]);
  readonly reports = this._reports.asReadonly();

  setReports(reports: ReportDto[]): void {
    this._reports.set(reports);
  }

  /** Retire un signalement traité de la file (optimiste). */
  removeById(id: string): void {
    this._reports.update((list) => list.filter((r) => r.id !== id));
  }

  reset(): void {
    this._reports.set([]);
  }
}
