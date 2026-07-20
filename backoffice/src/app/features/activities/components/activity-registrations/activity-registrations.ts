import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import QRCode from 'qrcode';
import { EditActivity } from '../../services/application/editActivity';
import { LoadActivityDashboard } from '../../services/application/loadActivityDashboard';
import { LoadRegistrations } from '../../services/application/loadRegistrations';
import { MarkAttendance } from '../../services/application/markAttendance';
import { RegistrationStates } from '../../services/infrastructure/states/registrationStates';

/** Écran « Inscrits & présence » d'une activité : liste + marquer présents. */
@Component({
  selector: 'app-activity-registrations',
  standalone: true,
  imports: [DatePipe, RouterModule, FormsModule],
  templateUrl: './activity-registrations.html',
})
export class ActivityRegistrations implements OnInit {
  readonly states = inject(RegistrationStates);
  readonly load = inject(LoadRegistrations);
  readonly mark = inject(MarkAttendance);
  readonly stats = inject(LoadActivityDashboard);
  readonly edit = inject(EditActivity);
  private readonly route = inject(ActivatedRoute);

  activityId = '';
  readonly selected = signal<Set<string>>(new Set());

  /** Modale « QR d'émargement » + image générée (data URL). */
  readonly showQr = signal(false);
  readonly qrDataUrl = signal<string | null>(null);

  constructor() {
    // Génère le QR dès que la modale s'ouvre et que le détail (jeton) est là.
    // Payload scanné par l'app mobile : uqtrsante://checkin?a=<id>&k=<token>.
    effect(() => {
      const detail = this.edit.detail();
      if (!this.showQr() || !detail) return;
      const payload = `uqtrsante://checkin?a=${detail.id}&k=${detail.checkInToken}`;
      QRCode.toDataURL(payload, { width: 480, margin: 2 })
        .then((url) => this.qrDataUrl.set(url))
        .catch(() => this.qrDataUrl.set(null));
    });
  }

  /** Filtre de statut actif (null = tous), piloté par le clic sur un KPI. */
  readonly statusFilter = signal<string | null>(null);
  /** Recherche libre sur nom / courriel. */
  readonly search = signal('');

  /** Lignes après filtre KPI + recherche. */
  readonly filteredRows = computed(() => {
    const status = this.statusFilter();
    const q = this.search().trim().toLowerCase();
    return this.states.rows().filter((r) => {
      if (status && r.status !== status) return false;
      if (q) {
        const hay = `${r.name ?? ''} ${r.email ?? ''}`.toLowerCase();
        if (!hay.includes(q)) return false;
      }
      return true;
    });
  });

  /** Clic sur un KPI : bascule le filtre (re-clic = tous). */
  setFilter(status: string | null): void {
    this.statusFilter.set(this.statusFilter() === status ? null : status);
  }

  ngOnInit(): void {
    this.activityId = this.route.snapshot.paramMap.get('id') ?? '';
    this.states.reset();
    this.load.handler(this.activityId);
    this.stats.handler(this.activityId);
    // Détail (titre, coût, jeton d'émargement) pour la modale QR.
    this.edit.load(this.activityId);
  }

  /** Ratio 0..1 → pourcentage entier. */
  pct(rate: number): string {
    return `${Math.round(rate * 100)}%`;
  }

  toggle(userId: string): void {
    const next = new Set(this.selected());
    if (next.has(userId)) next.delete(userId);
    else next.add(userId);
    this.selected.set(next);
  }

  isSelected(userId: string): boolean {
    return this.selected().has(userId);
  }

  selectAll(): void {
    // Sélectionne les lignes actuellement visibles (filtre + recherche).
    this.selected.set(new Set(this.filteredRows().map((r) => r.userId)));
  }

  clearSelection(): void {
    this.selected.set(new Set());
  }

  markPresent(): void {
    this.mark.handler(this.activityId, [...this.selected()], () => this.clearSelection());
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      registered: 'Inscrit',
      attended: 'Présent',
      waitlisted: "Liste d'attente",
      noshow: 'Absent',
    };
    return map[status] ?? status;
  }

  statusBadge(status: string): string {
    const map: Record<string, string> = {
      registered: 'badge-info',
      attended: 'badge-success',
      waitlisted: 'badge-warning',
      noshow: 'badge-error',
    };
    return map[status] ?? 'badge-ghost';
  }
}
