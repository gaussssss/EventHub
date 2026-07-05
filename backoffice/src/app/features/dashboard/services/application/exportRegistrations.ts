import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { DashboardService } from '../infrastructure/repository/dashboard';

/** Cas d'usage : télécharger l'export CSV des inscriptions. */
@Injectable({ providedIn: 'root' })
export class ExportRegistrations {
  public readonly isExporting = signal(false);

  private readonly repo = inject(DashboardService);
  private readonly toasts = inject(ToastStates);

  handler(): void {
    this.isExporting.set(true);
    this.repo.exportRegistrationsCsv().subscribe({
      next: (blob) => {
        this.isExporting.set(false);
        this.triggerDownload(blob, 'registrations.csv');
        this.toasts.success('Export CSV téléchargé');
      },
      error: () => {
        this.isExporting.set(false);
        this.toasts.error("Échec de l'export CSV");
      },
    });
  }

  private triggerDownload(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
  }
}
