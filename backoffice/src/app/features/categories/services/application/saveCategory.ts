import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { httpErrorMessage } from '../../../../core/http/httpError';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { CategoryRequest } from '../../models/categoryRequest';
import { CategoryService } from '../infrastructure/repository/category';
import { LoadCategories } from './loadCategories';

/** Cas d'usage : créer (id null) ou mettre à jour une catégorie. */
@Injectable({ providedIn: 'root' })
export class SaveCategory {
  public readonly isSaving = signal(false);

  private readonly repo = inject(CategoryService);
  private readonly toasts = inject(ToastStates);
  private readonly loadCategories = inject(LoadCategories);

  handler(id: string | null, request: CategoryRequest, callback?: () => void): void {
    if (!request.slug.trim() || !request.label.trim()) {
      this.toasts.warning('Slug et libellé sont requis.');
      return;
    }

    const call: Observable<unknown> = id
      ? this.repo.update(id, request)
      : this.repo.create(request);

    this.isSaving.set(true);
    call.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.toasts.success(id ? 'Catégorie mise à jour' : 'Catégorie créée');
        callback?.();
        this.loadCategories.handler();
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.toasts.error(httpErrorMessage(err) ?? "Échec de l'enregistrement");
      },
    });
  }
}
