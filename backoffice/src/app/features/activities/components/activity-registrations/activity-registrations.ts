import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { LoadActivityDashboard } from '../../services/application/loadActivityDashboard';
import { LoadRegistrations } from '../../services/application/loadRegistrations';
import { MarkAttendance } from '../../services/application/markAttendance';
import { RegistrationStates } from '../../services/infrastructure/states/registrationStates';

/** Écran « Inscrits & présence » d'une activité : liste + marquer présents. */
@Component({
  selector: 'app-activity-registrations',
  standalone: true,
  imports: [DatePipe, RouterModule],
  templateUrl: './activity-registrations.html',
})
export class ActivityRegistrations implements OnInit {
  readonly states = inject(RegistrationStates);
  readonly load = inject(LoadRegistrations);
  readonly mark = inject(MarkAttendance);
  readonly stats = inject(LoadActivityDashboard);
  private readonly route = inject(ActivatedRoute);

  activityId = '';
  readonly selected = signal<Set<string>>(new Set());

  ngOnInit(): void {
    this.activityId = this.route.snapshot.paramMap.get('id') ?? '';
    this.states.reset();
    this.load.handler(this.activityId);
    this.stats.handler(this.activityId);
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
    this.selected.set(new Set(this.states.rows().map((r) => r.userId)));
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
