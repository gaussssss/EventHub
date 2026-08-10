import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LoadLeaderboard } from '../../services/application/loadLeaderboard';
import { LeaderboardStates } from '../../services/infrastructure/states/leaderboardStates';

/** Écran « Classement » : classement global des cœurs (lecture seule, paginé). */
@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './leaderboard.html',
})
export class Leaderboard implements OnInit {
  readonly states = inject(LeaderboardStates);
  readonly load = inject(LoadLeaderboard);

  /** Recherche libre sur le nom (filtre la page affichée). */
  readonly search = signal('');

  readonly filteredRows = computed(() => {
    const q = this.search().trim().toLowerCase();
    if (!q) return this.states.rows();
    return this.states.rows().filter((r) =>
      (r.name ?? '').toLowerCase().includes(q));
  });

  ngOnInit(): void {
    this.load.handler(1);
  }

  next(): void {
    this.load.handler(this.load.page() + 1);
  }

  prev(): void {
    if (this.load.page() > 1) this.load.handler(this.load.page() - 1);
  }

  initials(name: string | null | undefined): string {
    const n = (name ?? '').trim();
    if (!n) return '?';
    const parts = n.split(/\s+/);
    return (parts[0]![0]! + (parts[1]?.[0] ?? '')).toUpperCase();
  }

  medal(rank: number): string {
    return rank === 1 ? '🥇' : rank === 2 ? '🥈' : rank === 3 ? '🥉' : '';
  }
}
