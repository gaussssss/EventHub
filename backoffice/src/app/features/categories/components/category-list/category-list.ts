import { Component, inject, OnInit } from '@angular/core';
import { ModalStates } from '../../../../shared/services/infrastructure/states/modalStates';
import { DeleteCategory } from '../../services/application/deleteCategory';
import { LoadCategories } from '../../services/application/loadCategories';
import { CategoryStates } from '../../services/infrastructure/states/categoryStates';
import { CategoryDto } from '../../models/categoryDto';
import { CategoryFormModal } from '../modals/category-form/category-form';

/** Écran « Catégories » : liste + création / édition / suppression. */
@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CategoryFormModal],
  templateUrl: './category-list.html',
})
export class CategoryList implements OnInit {
  readonly states = inject(CategoryStates);
  readonly load = inject(LoadCategories);
  readonly del = inject(DeleteCategory);
  readonly modals = inject(ModalStates);

  ngOnInit(): void {
    this.load.handler();
  }

  openCreate(): void {
    this.modals.open('category-form', {
      id: null,
      slug: '',
      label: '',
      color: '#0078d4',
      icon: '',
    });
  }

  openEdit(category: CategoryDto): void {
    this.modals.open('category-form', {
      id: category.id,
      slug: category.slug,
      label: category.label,
      color: category.color ?? '#0078d4',
      icon: category.icon ?? '',
    });
  }

  confirmDelete(category: CategoryDto): void {
    if (window.confirm(`Supprimer la catégorie « ${category.label} » ?`)) {
      this.del.handler(category.id);
    }
  }
}
