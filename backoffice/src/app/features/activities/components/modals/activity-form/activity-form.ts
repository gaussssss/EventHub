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
        startsAt: this.toUtcIso(this.startsAt()) ?? '',
        endsAt: this.toUtcIso(this.endsAt()),
        location: this.location().trim(),
        imageUrl: this.imageUrl().trim(),
        heartsReward: Number(this.heartsReward()) || 0,
        maxParticipants: Number(this.maxParticipants()) || 0,
        participationCost: Number(this.participationCost()) || 0,
        registrationUrl: this.registrationUrl().trim(),
        registrationDeadline: this.toUtcIso(this.registrationDeadline()),
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

  /**
   * Convention de fuseau : l'API stocke et sert de l'**UTC** ; le back-office
   * saisit et affiche en **heure locale** du navigateur. Ces deux helpers font
   * la conversion aux frontières (et uniquement là).
   */

  /** Valeur d'un input datetime-local (heure LOCALE) → ISO UTC pour l'API. */
  private toUtcIso(local: string): string | null {
    // `new Date("YYYY-MM-DDTHH:mm")` interprète la valeur en heure locale.
    return local ? new Date(local).toISOString() : null;
  }

  /** ISO UTC de l'API → valeur d'un input datetime-local en heure LOCALE. */
  private toInput(iso: string | null | undefined): string {
    if (!iso) return '';
    const d = new Date(iso);
    const p = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}` +
      `T${p(d.getHours())}:${p(d.getMinutes())}`;
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
