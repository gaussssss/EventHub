import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { CategoryService } from '../infrastructure/repository/category';
import { CategoryStates } from '../infrastructure/states/categoryStates';

/** Cas d'usage : charger la liste des catégories. */
@Injectable({ providedIn: 'root' })
export class LoadCategories {
  public readonly isLoading = signal(false);

  private readonly repo = inject(CategoryService);
  private readonly states = inject(CategoryStates);
  private readonly toasts = inject(ToastStates);

  handler(): void {
    this.isLoading.set(true);
    this.repo.getAll().subscribe({
      next: (categories) => {
        this.isLoading.set(false);
        this.states.setCategories(categories);
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement des catégories');
      },
    });
  }
}
