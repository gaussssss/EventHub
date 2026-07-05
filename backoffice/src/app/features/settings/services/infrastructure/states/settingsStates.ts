import { Injectable, signal } from '@angular/core';
import { GamificationSettingsDto } from '../../../models/gamificationSettingsDto';

/** Store signal des réglages de gamification. */
@Injectable({ providedIn: 'root' })
export class SettingsStates {
  private readonly _gamification = signal<GamificationSettingsDto | null>(null);
  readonly gamification = this._gamification.asReadonly();

  setGamification(settings: GamificationSettingsDto): void {
    this._gamification.set(settings);
  }
}
