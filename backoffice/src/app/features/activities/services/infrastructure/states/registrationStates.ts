import { Injectable, signal } from '@angular/core';
import { RegistrationEntryDto } from '../../../models/registrationEntryDto';

/** Store signal des inscrits de l'activité en cours de consultation. */
@Injectable({ providedIn: 'root' })
export class RegistrationStates {
  private readonly _rows = signal<RegistrationEntryDto[]>([]);
  readonly rows = this._rows.asReadonly();

  setRows(rows: RegistrationEntryDto[]): void {
    this._rows.set(rows);
  }

  reset(): void {
    this._rows.set([]);
  }
}
