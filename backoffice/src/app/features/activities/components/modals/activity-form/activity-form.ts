import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalStates } from '../../../../../shared/services/infrastructure/states/modalStates';
import { LoadCategories } from '../../../../categories/services/application/loadCategories';
import { CategoryStates } from '../../../../categories/services/infrastructure/states/categoryStates';
import { LoadOrganizers } from '../../../../organizers/services/application/loadOrganizers';
import { OrganizerStates } from '../../../../organizers/services/infrastructure/states/organizerStates';
import { EditActivity } from '../../../services/application/editActivity';
import { SaveActivity } from '../../../services/application/saveActivity';

/** Modale création / édition d'une activité (formulaire complet). */
@Component({
  selector: 'app-activity-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './activity-form.html',
})
export class ActivityFormModal {
  readonly modals = inject(ModalStates);
  readonly save = inject(SaveActivity);
  readonly edit = inject(EditActivity);
  readonly categories = inject(CategoryStates);
  readonly organizers = inject(OrganizerStates);
  private readonly loadCategories = inject(LoadCategories);
  private readonly loadOrganizers = inject(LoadOrganizers);

  readonly data = computed(() => this.modals.getData('activity-form'));
  readonly isEdit = computed(() => this.data()?.id != null);

  readonly title = signal('');
  readonly description = signal('');
  readonly categoryId = signal('');
  readonly organizerId = signal('');
  readonly startsAt = signal('');
  readonly endsAt = signal('');
  readonly location = signal('');
  readonly imageUrl = signal('');
  readonly heartsReward = signal(0);
  readonly maxParticipants = signal(0);
  readonly participationCost = signal(0);
  readonly registrationUrl = signal('');
  readonly registrationDeadline = signal('');
  readonly isFeatured = signal(false);
  readonly status = signal('draft');

  /** Passe à true au 1er blur / tentative de soumission (affichage des erreurs). */
  readonly urlTouched = signal(false);

  /** Message d'erreur du lien d'inscription (obligatoire + format URL), ou null. */
  readonly registrationUrlError = computed(() => {
    const v = this.registrationUrl().trim();
    if (!v) return "Le lien d'inscription est obligatoire.";
    let url: URL;
    try {
      url = new URL(v);
    } catch {
      return 'Adresse URL invalide (ex. https://…).';
    }
    if (url.protocol !== 'http:' && url.protocol !== 'https:') {
      return "L'URL doit commencer par http:// ou https://.";
    }
    return null;
  });

  constructor() {
    // À l'ouverture : charge les référentiels, puis réinitialise (création) ou
    // déclenche le chargement du détail (édition).
    effect(() => {
      const d = this.data();
      if (!d) return;
      if (this.categories.categories().length === 0) this.loadCategories.handler();
      if (this.organizers.organizers().length === 0) this.loadOrganizers.handler();
      if (d.id) {
        this.edit.load(d.id);
      } else {
        this.reset();
      }
    });

    // Quand le détail arrive (édition), pré-remplit le formulaire.
    effect(() => {
      const detail = this.edit.detail();
      if (!detail) return;
      this.title.set(detail.title);
      this.description.set(detail.description);
      this.categoryId.set(detail.categoryId);
      this.organizerId.set(detail.organizerId ?? '');
      this.startsAt.set(this.toInput(detail.startsAt));
      this.endsAt.set(this.toInput(detail.endsAt));
      this.location.set(detail.location);
      this.imageUrl.set(detail.imageUrl);
      this.heartsReward.set(detail.heartsReward);
      this.maxParticipants.set(detail.maxParticipants);
      this.participationCost.set(detail.participationCost ?? 0);
      this.registrationUrl.set(detail.registrationUrl ?? '');
      this.registrationDeadline.set(this.toInput(detail.registrationDeadline));
      this.isFeatured.set(detail.isFeatured);
      this.status.set(detail.status);
    });
  }

  submit(): void {
    // Le lien d'inscription est obligatoire et doit être une URL valide.
    this.urlTouched.set(true);
    if (this.registrationUrlError()) return;

    this.save.handler(
      this.data()?.id ?? null,
      {
        title: this.title().trim(),
        description: this.description().trim(),
        categoryId: this.categoryId(),
        organizerId: this.organizerId() || null,
        startsAt: this.startsAt(),
        endsAt: this.endsAt() || null,
        location: this.location().trim(),
        imageUrl: this.imageUrl().trim(),
        heartsReward: Number(this.heartsReward()) || 0,
        maxParticipants: Number(this.maxParticipants()) || 0,
        participationCost: Number(this.participationCost()) || 0,
        registrationUrl: this.registrationUrl().trim(),
        registrationDeadline: this.registrationDeadline() || null,
        isFeatured: this.isFeatured(),
        status: this.status(),
      },
      () => this.close(),
    );
  }

  close(): void {
    this.modals.close('activity-form');
    this.edit.clear();
  }

  /** ISO → valeur d'un input datetime-local (YYYY-MM-DDTHH:mm). */
  private toInput(iso: string | null | undefined): string {
    return iso ? iso.slice(0, 16) : '';
  }

  private reset(): void {
    this.title.set('');
    this.description.set('');
    this.categoryId.set('');
    this.organizerId.set('');
    this.startsAt.set('');
    this.endsAt.set('');
    this.location.set('');
    this.imageUrl.set('');
    this.heartsReward.set(0);
    this.maxParticipants.set(0);
    this.participationCost.set(0);
    this.registrationUrl.set('');
    this.registrationDeadline.set('');
    this.isFeatured.set(false);
    this.status.set('draft');
    this.urlTouched.set(false);
  }
}
