import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { CategoryService } from '../infrastructure/repository/category';
import { LoadCategories } from './loadCategories';

/** Cas d'usage : supprimer une catégorie. */
@Injectable({ providedIn: 'root' })
export class DeleteCategory {
  public readonly deletingId = signal<string | null>(null);

  private readonly repo = inject(CategoryService);
  private readonly toasts = inject(ToastStates);
  private readonly loadCategories = inject(LoadCategories);

  handler(id: string): void {
    this.deletingId.set(id);
    this.repo.delete(id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toasts.success('Catégorie supprimée');
        this.loadCategories.handler();
      },
      error: () => {
        this.deletingId.set(null);
        this.toasts.error('Échec de la suppression');
      },
    });
  }
}
