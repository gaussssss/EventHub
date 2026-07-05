import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalStates } from '../../../../../shared/services/infrastructure/states/modalStates';
import { SaveOrganizer } from '../../../services/application/saveOrganizer';

/** Modale création / édition d'un organisateur (`ModalStates('organizer-form')`). */
@Component({
  selector: 'app-organizer-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './organizer-form.html',
})
export class OrganizerFormModal {
  readonly modals = inject(ModalStates);
  readonly save = inject(SaveOrganizer);

  readonly name = signal('');
  readonly contactEmail = signal('');

  readonly data = computed(() => this.modals.getData('organizer-form'));
  readonly isEdit = computed(() => this.data()?.id != null);

  constructor() {
    effect(() => {
      const d = this.data();
      if (!d) return;
      this.name.set(d.name);
      this.contactEmail.set(d.contactEmail);
    });
  }

  submit(): void {
    const d = this.data();
    this.save.handler(
      d?.id ?? null,
      { name: this.name().trim(), contactEmail: this.contactEmail().trim() || null },
      () => this.close(),
    );
  }

  close(): void {
    this.modals.close('organizer-form');
  }
}
