import { Injectable, signal } from '@angular/core';

/**
 * Registre **typé** des modales ouvertes. Chaque id de modale est associé à la
 * forme de ses données via `ModalData`, ce qui rend `open`/`getData` type-safe.
 * Ajouter une modale = ajouter une entrée ici.
 */
export type ModalData = {
  'update-user': { id: string; name: string; role: string; status: string };
  'award-hearts': { id: string; name: string; totalHearts: number };
  'category-form': {
    id: string | null;
    slug: string;
    label: string;
    color: string;
    icon: string;
  };
  'activity-form': { id: string | null };
  'organizer-form': { id: string | null; name: string; contactEmail: string };
  'moderation-hide': {
    reportId: string;
    targetType: string;
    targetId: string;
    authorName: string;
    preview: string;
    imageUrl: string;
    reason: string;
  };
};

export type ModalId = keyof ModalData;

@Injectable({ providedIn: 'root' })
export class ModalStates {
  private readonly activeModals = signal<Map<ModalId, unknown>>(new Map());

  open<T extends ModalId>(modalId: T, data: ModalData[T]): void {
    this.activeModals.update((modals) => new Map([...modals, [modalId, data]]));
  }

  getData<T extends ModalId>(id: T): ModalData[T] | undefined {
    return this.activeModals().get(id) as ModalData[T] | undefined;
  }

  isOpen(modalId: ModalId): boolean {
    return this.activeModals().has(modalId);
  }

  close(id: ModalId): void {
    this.activeModals.update((modals) => {
      const next = new Map(modals);
      next.delete(id);
      return next;
    });
  }
}
