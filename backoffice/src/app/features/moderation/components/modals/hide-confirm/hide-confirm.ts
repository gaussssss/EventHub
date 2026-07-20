import { Component, computed, inject } from '@angular/core';
import { ModalStates } from '../../../../../shared/services/infrastructure/states/modalStates';
import { HideTarget } from '../../../services/application/hideTarget';
import { resolveMediaUrl } from '../../../../../core/utils/media-url';

/** Modale de confirmation + prévisualisation avant de masquer un contenu signalé. */
@Component({
  selector: 'app-hide-confirm',
  standalone: true,
  templateUrl: './hide-confirm.html',
})
export class HideConfirmModal {
  readonly modals = inject(ModalStates);
  readonly hide = inject(HideTarget);

  readonly data = computed(() => this.modals.getData('moderation-hide'));
  readonly isComment = computed(() => (this.data()?.targetType ?? '').toLowerCase() === 'comment');
  readonly busy = computed(() => this.hide.hidingId() === this.data()?.reportId);

  /** URL d'image affichable (ré-ancrée sur l'origine API, cf. resolveMediaUrl). */
  mediaUrl(url: string | null | undefined): string {
    return resolveMediaUrl(url);
  }

  confirm(): void {
    const d = this.data();
    if (!d) return;
    this.hide.handler(d.reportId, d.targetType, d.targetId, () => this.close());
  }

  close(): void {
    this.modals.close('moderation-hide');
  }
}
