import { Component, inject, OnInit } from '@angular/core';
import { LoadLeaderboard } from '../../services/application/loadLeaderboard';
import { LeaderboardStates } from '../../services/infrastructure/states/leaderboardStates';

/** Écran « Classement » : classement global des cœurs (lecture seule, paginé). */
@Component({
  selector: 'app-leaderboard',
  standalone: true,
  templateUrl: './leaderboard.html',
})
export class Leaderboard implements OnInit {
  readonly states = inject(LeaderboardStates);
  readonly load = inject(LoadLeaderboard);

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
