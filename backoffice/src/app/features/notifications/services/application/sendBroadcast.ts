import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { BroadcastRequest } from '../../models/broadcastRequest';
import { NotificationService } from '../infrastructure/repository/notification';

/** Cas d'usage : diffuser une notification à une audience. */
@Injectable({ providedIn: 'root' })
export class SendBroadcast {
  public readonly isSending = signal(false);

  private readonly repo = inject(NotificationService);
  private readonly toasts = inject(ToastStates);

  handler(body: BroadcastRequest, callback?: () => void): void {
    if (!body.title.trim() || !body.body.trim()) {
      this.toasts.warning('Titre et message sont requis.');
      return;
    }

    this.isSending.set(true);
    this.repo.broadcast(body).subscribe({
      next: (res) => {
        this.isSending.set(false);
        this.toasts.success(`Notification envoyée à ${res.recipients} destinataire(s).`);
        callback?.();
      },
      error: (err: HttpErrorResponse) => {
        this.isSending.set(false);
        this.toasts.error(err.status === 400 ? 'Titre et message requis.' : "Échec de l'envoi");
      },
    });
  }
}
