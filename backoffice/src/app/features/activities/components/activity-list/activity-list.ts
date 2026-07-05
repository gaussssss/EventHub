import { DatePipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ModalStates } from '../../../../shared/services/infrastructure/states/modalStates';
import { ActivityActions } from '../../services/application/activityActions';
import { LoadActivities } from '../../services/application/loadActivities';
import { ActivityStates } from '../../services/infrastructure/states/activityStates';
import { AdminActivityDto } from '../../models/adminActivityDto';
import { ActivityFormModal } from '../modals/activity-form/activity-form';

/** Écran « Activités » : liste (tous statuts) + création + publier / annuler / à la une. */
@Component({
  selector: 'app-activity-list',
  standalone: true,
  imports: [DatePipe, RouterModule, ActivityFormModal],
  templateUrl: './activity-list.html',
})
export class ActivityList implements OnInit {
  readonly states = inject(ActivityStates);
  readonly load = inject(LoadActivities);
  readonly actions = inject(ActivityActions);
  readonly modals = inject(ModalStates);

  ngOnInit(): void {
    this.load.handler();
  }

  openCreate(): void {
    this.modals.open('activity-form', { id: null });
  }

  openEdit(activity: AdminActivityDto): void {
    this.modals.open('activity-form', { id: activity.id });
  }

  isPublished(status: string): boolean {
    return status === 'published';
  }

  featureIcon(isFeatured: boolean): string {
    return isFeatured ? 'icon-[fluent--star-24-filled]' : 'icon-[fluent--star-24-regular]';
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      published: 'Publiée',
      draft: 'Brouillon',
      cancelled: 'Annulée',
      archived: 'Archivée',
    };
    return map[status] ?? status;
  }

  statusBadge(status: string): string {
    const map: Record<string, string> = {
      published: 'badge-success',
      draft: 'badge-ghost',
      cancelled: 'badge-error',
      archived: 'badge-warning',
    };
    return map[status] ?? 'badge-ghost';
  }

  confirmCancel(activity: AdminActivityDto): void {
    if (window.confirm(`Annuler l'activité « ${activity.title} » ?`)) {
      this.actions.cancel(activity.id);
    }
  }
}
