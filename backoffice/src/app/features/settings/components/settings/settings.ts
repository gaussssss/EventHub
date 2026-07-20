import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LoadGamification } from '../../services/application/loadGamification';
import { ManageContributors } from '../../services/application/manageContributors';
import { SaveGamification } from '../../services/application/saveGamification';
import { SettingsStates } from '../../services/infrastructure/states/settingsStates';

/** Écran « Paramètres » : gamification (seuils) + contributeurs « À propos ». */
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
  readonly contributors = inject(ManageContributors);

  readonly silver = signal(0);
  readonly gold = signal(0);

  constructor() {
    effect(() => {
      const s = this.states.gamification();
      if (!s) return;
      this.silver.set(s.silverThreshold);
      this.gold.set(s.goldThreshold);
    });
  }

  ngOnInit(): void {
    this.load.handler();
    this.contributors.load();
  }

  submit(): void {
    // Le contrat de l'API attend encore defaultAttendanceReward ; il n'est plus
    // exposé dans l'UI (les cœurs sont définis par activité), donc on renvoie la
    // valeur existante inchangée plutôt que de l'écraser.
    this.save.handler({
      silverThreshold: Number(this.silver()) || 0,
      goldThreshold: Number(this.gold()) || 0,
      defaultAttendanceReward:
        this.states.gamification()?.defaultAttendanceReward ?? 0,
    });
  }
}
