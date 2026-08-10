import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { resolveMediaUrl } from '../../../../../core/utils/media-url';
import { ModalStates } from '../../../../../shared/services/infrastructure/states/modalStates';
import { ToastStates } from '../../../../../shared/services/infrastructure/states/toastStates';
import { LoadCategories } from '../../../../categories/services/application/loadCategories';
import { CategoryStates } from '../../../../categories/services/infrastructure/states/categoryStates';
import { LoadOrganizers } from '../../../../organizers/services/application/loadOrganizers';
import { OrganizerStates } from '../../../../organizers/services/infrastructure/states/organizerStates';
import { EditActivity } from '../../../services/application/editActivity';
import { SaveActivity } from '../../../services/application/saveActivity';
import { UploadsService } from '../../../services/infrastructure/repository/uploads';

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
  private readonly uploads = inject(UploadsService);
  private readonly toasts = inject(ToastStates);

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

  /** Vrai après une tentative d'enregistrement (affiche le panneau d'erreurs). */
  readonly submitAttempted = signal(false);

  // --- Image : URL externe OU fichier téléversé depuis le poste -------------

  /** Source de l'image, pilotée par la case « Depuis mon ordinateur ». */
  readonly imageSource = signal<'url' | 'file'>('url');

  /** Fichier choisi (mode fichier) ; `null` = conserver l'image actuelle. */
  readonly imageFile = signal<File | null>(null);

  /** Aperçu affiché sous le sélecteur (data URL ou image actuelle résolue). */
  readonly imagePreview = signal<string | null>(null);

  /** Téléversement en cours (bloque le bouton Enregistrer). */
  readonly isUploading = signal(false);

  setImageFromPc(fromPc: boolean): void {
    this.imageSource.set(fromPc ? 'file' : 'url');
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.imageFile.set(file);
    if (!file) {
      this.imagePreview.set(null);
      return;
    }
    const reader = new FileReader();
    reader.onload = () => this.imagePreview.set(reader.result as string);
    reader.readAsDataURL(file);
  }

  /**
   * Contraintes d'édition : champs obligatoires, cohérence des dates (fin après
   * début, échéance d'inscription avant le début), bornes numériques. La liste
   * est vide quand le formulaire est valide ; l'enregistrement est bloqué sinon.
   * (Le lien d'inscription a sa propre validation inline, registrationUrlError.)
   */
  readonly formErrors = computed(() => {
    const errors: string[] = [];
    if (!this.title().trim()) errors.push('Le titre est requis.');
    if (!this.description().trim()) errors.push('La description est requise.');
    if (!this.categoryId()) errors.push('La catégorie est requise.');
    if (!this.location().trim()) errors.push('Le lieu est requis.');

    const image = this.imageUrl().trim();
    if (this.imageSource() === 'file') {
      // Nouveau fichier requis, sauf en édition où l'image actuelle est gardée.
      if (!this.imageFile() && !image) {
        errors.push('Choisissez une image depuis votre ordinateur.');
      }
    } else if (!image) {
      errors.push("L'image (URL) est requise.");
    } else if (!/^https?:\/\//i.test(image) && !image.startsWith('/uploads/')) {
      errors.push("L'image doit être une URL http(s) valide.");
    }

    if (!this.startsAt()) errors.push('La date de début est requise.');
    const starts = this.startsAt() ? new Date(this.startsAt()) : null;
    const ends = this.endsAt() ? new Date(this.endsAt()) : null;
    const deadline = this.registrationDeadline()
      ? new Date(this.registrationDeadline())
      : null;
    if (starts && ends && ends <= starts) {
      errors.push('La fin doit être postérieure au début.');
    }
    if (starts && deadline && deadline > starts) {
      errors.push("La date limite d'inscription doit précéder le début.");
    }

    if ((Number(this.maxParticipants()) || 0) < 1) {
      errors.push('Le nombre de places doit être au moins 1.');
    }
    if (Number(this.heartsReward()) < 0) {
      errors.push('Les cœurs (récompense) ne peuvent pas être négatifs.');
    }
    return errors;
  });

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
      // Image déjà téléversée (/uploads/…) → mode fichier avec aperçu de
      // l'actuelle (conservée tant qu'aucun nouveau fichier n'est choisi).
      const uploaded = detail.imageUrl.startsWith('/uploads/');
      this.imageSource.set(uploaded ? 'file' : 'url');
      this.imageFile.set(null);
      this.imagePreview.set(uploaded ? resolveMediaUrl(detail.imageUrl) : null);
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
    // Contraintes d'édition : tout doit être valide avant l'enregistrement.
    this.urlTouched.set(true);
    this.submitAttempted.set(true);
    if (this.formErrors().length > 0 || this.registrationUrlError()) return;

    // Mode fichier avec un nouveau fichier : téléverser d'abord, puis
    // enregistrer l'activité avec le chemin renvoyé (/uploads/…). Sans nouveau
    // fichier (édition), l'image actuelle est conservée telle quelle.
    const file = this.imageSource() === 'file' ? this.imageFile() : null;
    if (file) {
      this.isUploading.set(true);
      this.uploads.uploadImage(file).subscribe({
        next: ({ url }) => {
          this.isUploading.set(false);
          this.persist(url);
        },
        error: () => {
          this.isUploading.set(false);
          this.toasts.error("Échec du téléversement de l'image.");
        },
      });
      return;
    }
    this.persist(this.imageUrl().trim());
  }

  private persist(imageUrl: string): void {
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
        imageUrl,
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
    this.submitAttempted.set(false);
    this.imageSource.set('url');
    this.imageFile.set(null);
    this.imagePreview.set(null);
    this.isUploading.set(false);
  }
}
