import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalStates } from '../../../../../shared/services/infrastructure/states/modalStates';
import { AwardHearts } from '../../../services/application/awardHearts';

/** Modale d'ajustement des cœurs, pilotée par `ModalStates('award-hearts')`. */
@Component({
  selector: 'app-award-hearts',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './award-hearts.html',
})
export class AwardHeartsModal {
  readonly modals = inject(ModalStates);
  readonly award = inject(AwardHearts);

  readonly hearts = signal(0);
  readonly reason = signal('');

  readonly data = computed(() => this.modals.getData('award-hearts'));

  submit(): void {
    const target = this.modals.getData('award-hearts');
    if (!target) return;
    this.award.handler(
      target.id,
      { hearts: this.hearts(), reason: this.reason().trim() || undefined },
      () => this.close(),
    );
  }

  close(): void {
    this.modals.close('award-hearts');
    this.hearts.set(0);
    this.reason.set('');
  }
}
