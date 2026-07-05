import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LoadGamification } from '../../services/application/loadGamification';
import { SaveGamification } from '../../services/application/saveGamification';
import { SettingsStates } from '../../services/infrastructure/states/settingsStates';

/** Écran « Paramètres » : réglages de gamification (seuils + récompense). */
@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './settings.html',
})
export class Settings implements OnInit {
  readonly states = inject(SettingsStates);
  readonly load = inject(LoadGamification);
  readonly save = inject(SaveGamification);

  readonly silver = signal(0);
  readonly gold = signal(0);
  readonly reward = signal(0);

  constructor() {
    effect(() => {
      const s = this.states.gamification();
      if (!s) return;
      this.silver.set(s.silverThreshold);
      this.gold.set(s.goldThreshold);
      this.reward.set(s.defaultAttendanceReward);
    });
  }

  ngOnInit(): void {
    this.load.handler();
  }

  submit(): void {
    this.save.handler({
      silverThreshold: Number(this.silver()) || 0,
      goldThreshold: Number(this.gold()) || 0,
      defaultAttendanceReward: Number(this.reward()) || 0,
    });
  }
}
