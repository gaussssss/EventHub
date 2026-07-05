import { Injectable, signal } from '@angular/core';
import { CategoryDto } from '../../../models/categoryDto';

/** Store signal des catégories. */
@Injectable({ providedIn: 'root' })
export class CategoryStates {
  private readonly _categories = signal<CategoryDto[]>([]);
  readonly categories = this._categories.asReadonly();

  setCategories(categories: CategoryDto[]): void {
    this._categories.set(categories);
  }

  reset(): void {
    this._categories.set([]);
  }
}
