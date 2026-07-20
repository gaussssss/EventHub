import { inject, Injectable, signal } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { ContributorDto } from '../../models/contributorDto';
import { ContributorsService } from '../infrastructure/repository/contributors';

/**
 * Ligne éditable du tableau des contributeurs : `id` null = pas encore créé
 * côté serveur (sera POSTé à l'enregistrement, sinon PATCHé).
 */
export interface ContributorRow {
  id: string | null;
  name: string;
  role: string;
  avatarUrl: string;
  sortOrder: number;
}

/** Cas d'usage : gérer les contributeurs de la page « À propos » (CRUD). */
@Injectable({ providedIn: 'root' })
export class ManageContributors {
  readonly rows = signal<ContributorRow[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);

  private readonly repo = inject(ContributorsService);
  private readonly toasts = inject(ToastStates);

  load(): void {
    this.isLoading.set(true);
    this.repo.getAll().subscribe({
      next: (list) => {
        this.isLoading.set(false);
        this.rows.set(list.map((c) => this.toRow(c)));
      },
      error: () => {
        this.isLoading.set(false);
        this.toasts.error('Erreur de chargement des contributeurs');
      },
    });
  }

  addRow(): void {
    const nextOrder =
      Math.max(0, ...this.rows().map((r) => r.sortOrder)) + 1;
    this.rows.update((rows) => [
      ...rows,
      { id: null, name: '', role: '', avatarUrl: '', sortOrder: nextOrder },
    ]);
  }

  updateRow(index: number, patch: Partial<ContributorRow>): void {
    this.rows.update((rows) =>
      rows.map((r, i) => (i === index ? { ...r, ...patch } : r)),
    );
  }

  removeRow(index: number): void {
    const row = this.rows()[index];
    if (!row) return;
    // Ligne jamais persistée : retrait local immédiat.
    if (row.id === null) {
      this.rows.update((rows) => rows.filter((_, i) => i !== index));
      return;
    }
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.rows.update((rows) => rows.filter((_, i) => i !== index));
        this.toasts.success('Contributeur supprimé');
      },
      error: () => this.toasts.error('Erreur lors de la suppression'),
    });
  }

  /** Persiste toutes les lignes valides : POST les nouvelles, PATCH les existantes. */
  saveAll(): void {
    const valid = this.rows().filter(
      (r) => r.name.trim() !== '' && r.role.trim() !== '',
    );
    if (valid.length !== this.rows().length) {
      this.toasts.error('Nom et rôle sont requis pour chaque contributeur.');
      return;
    }
    if (valid.length === 0) return;

    const calls: Observable<unknown>[] = valid.map((r) => {
      const body = {
        name: r.name.trim(),
        role: r.role.trim(),
        avatarUrl: r.avatarUrl.trim() || null,
        sortOrder: Number(r.sortOrder) || 0,
      };
      return r.id === null ? this.repo.create(body) : this.repo.update(r.id, body);
    });

    this.isSaving.set(true);
    forkJoin(calls.length ? calls : [of(null)]).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.toasts.success('Contributeurs enregistrés');
        this.load(); // recharge (récupère les ids créés + l'ordre serveur)
      },
      error: () => {
        this.isSaving.set(false);
        this.toasts.error("Erreur lors de l'enregistrement des contributeurs");
      },
    });
  }

  private toRow(c: ContributorDto): ContributorRow {
    return {
      id: c.id,
      name: c.name,
      role: c.role,
      avatarUrl: c.avatarUrl ?? '',
      sortOrder: c.sortOrder,
    };
  }
}
