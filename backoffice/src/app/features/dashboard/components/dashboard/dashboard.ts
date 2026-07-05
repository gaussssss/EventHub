import { Component, computed, inject, OnInit } from '@angular/core';
import { environment } from '../../../../../environments/environment';
import { ExportRegistrations } from '../../services/application/exportRegistrations';
import { LoadDashboard } from '../../services/application/loadDashboard';
import { SeedDevData } from '../../services/application/seedDevData';
import { DashboardStates } from '../../services/infrastructure/states/dashboardStates';

interface KpiCard {
  label: string;
  value: number;
  icon: string;
  hint?: string;
  accent?: string;
}

/** Tableau de bord : cartes KPI (lecture seule) + export CSV des inscriptions. */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  readonly states = inject(DashboardStates);
  readonly load = inject(LoadDashboard);
  readonly export = inject(ExportRegistrations);
  readonly seed = inject(SeedDevData);

  /** Le bouton de seed n'existe qu'en développement. */
  readonly isDev = !environment.production;

  ngOnInit(): void {
    this.load.handler();
  }

  runSeed(): void {
    this.seed.handler(() => this.load.handler());
  }

  readonly cards = computed<KpiCard[]>(() => {
    const o = this.states.overview();
    if (!o) return [];
    return [
      {
        label: 'Utilisateurs',
        value: o.totalUsers,
        icon: 'icon-[fluent--people-24-regular]',
      },
      {
        label: 'Activités',
        value: o.totalActivities,
        icon: 'icon-[fluent--calendar-ltr-24-regular]',
        hint: `${o.publishedActivities} publiées · ${o.upcomingActivities} à venir`,
      },
      {
        label: 'Inscriptions',
        value: o.totalRegistrations,
        icon: 'icon-[fluent--clipboard-task-list-ltr-24-regular]',
        hint: `${o.waitlistedRegistrations} en liste d'attente`,
      },
      {
        label: 'Cœurs attribués',
        value: o.totalHeartsAwarded,
        icon: 'icon-[fluent--heart-24-filled]',
        accent: 'text-error',
      },
      {
        label: 'Publications',
        value: o.totalPosts,
        icon: 'icon-[fluent--comment-multiple-24-regular]',
      },
    ];
  });
}
