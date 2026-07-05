import { Injectable, signal } from '@angular/core';
import { OrganizerDto } from '../../../models/organizerDto';

/** Store signal des organisateurs. */
@Injectable({ providedIn: 'root' })
export class OrganizerStates {
  private readonly _organizers = signal<OrganizerDto[]>([]);
  readonly organizers = this._organizers.asReadonly();

  setOrganizers(organizers: OrganizerDto[]): void {
    this._organizers.set(organizers);
  }

  reset(): void {
    this._organizers.set([]);
  }
}
