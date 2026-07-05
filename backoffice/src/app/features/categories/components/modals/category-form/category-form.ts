import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalStates } from '../../../../../shared/services/infrastructure/states/modalStates';
import { SaveCategory } from '../../../services/application/saveCategory';

/** Modale création / édition d'une catégorie (`ModalStates('category-form')`). */
@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './category-form.html',
})
export class CategoryFormModal {
  readonly modals = inject(ModalStates);
  readonly save = inject(SaveCategory);

  readonly slug = signal('');
  readonly label = signal('');
  readonly color = signal('#0078d4');
  readonly icon = signal('');

  readonly data = computed(() => this.modals.getData('category-form'));
  readonly isEdit = computed(() => this.data()?.id != null);

  constructor() {
    // Pré-remplit le formulaire à l'ouverture (édition) et le réinitialise.
    effect(() => {
      const d = this.data();
      if (!d) return;
      this.slug.set(d.slug);
      this.label.set(d.label);
      this.color.set(d.color || '#0078d4');
      this.icon.set(d.icon);
    });
  }

  submit(): void {
    const d = this.data();
    this.save.handler(
      d?.id ?? null,
      {
        slug: this.slug().trim(),
        label: this.label().trim(),
        color: this.color().trim() || null,
        icon: this.icon().trim() || null,
      },
      () => this.close(),
    );
  }

  close(): void {
    this.modals.close('category-form');
  }
}
