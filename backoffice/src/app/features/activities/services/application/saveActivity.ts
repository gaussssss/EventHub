import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { httpErrorMessage } from '../../../../core/http/httpError';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ActivityRequest } from '../../models/activityRequest';
import { ActivityService } from '../infrastructure/repository/activity';
import { LoadActivities } from './loadActivities';

/** Cas d'usage : créer (id null) ou mettre à jour une activité. */
@Injectable({ providedIn: 'root' })
export class SaveActivity {
  public readonly isSaving = signal(false);

  private readonly repo = inject(ActivityService);
  private readonly toasts = inject(ToastStates);
  private readonly loadActivities = inject(LoadActivities);

  handler(id: string | null, body: ActivityRequest, callback?: () => void): void {
    const missing = this.missingFields(body);
    if (missing.length > 0) {
      this.toasts.warning(`Champs obligatoires manquants : ${missing.join(', ')}.`);
      return;
    }

    const call: Observable<unknown> = id
      ? this.repo.update(id, body)
      : this.repo.create(body);

    this.isSaving.set(true);
    call.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.toasts.success(id ? 'Activité mise à jour' : 'Activité créée');
        callback?.();
        this.loadActivities.handler();
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        // Affiche le vrai message de l'API (ProblemDetails.detail / {error}),
        // pour coller à ce qui apparaît en console.
        this.toasts.error(httpErrorMessage(err) ?? "Échec de l'enregistrement");
      },
    });
  }

  /** Champs requis par le domaine (cf. Activity.Create) non renseignés. */
  private missingFields(body: ActivityRequest): string[] {
    const missing: string[] = [];
    if (!body.title.trim()) missing.push('titre');
    if (!body.description.trim()) missing.push('description');
    if (!body.categoryId) missing.push('catégorie');
    if (!body.startsAt) missing.push('date de début');
    if (!body.location.trim()) missing.push('lieu');
    if (!body.imageUrl.trim()) missing.push('image (URL)');
    if (!body.maxParticipants || body.maxParticipants < 1) missing.push('places max (≥ 1)');
    return missing;
  }
}
