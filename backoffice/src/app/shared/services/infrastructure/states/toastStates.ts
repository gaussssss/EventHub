import { Injectable, signal } from '@angular/core';
import { IToast } from '../../../models/interfaces/IToast';
import { ToastType } from '../../../models/enums/toastType';

/**
 * File de notifications (toasts). Store signal pur : liste privée exposée en
 * lecture seule, ajout avec id auto-incrémenté et auto-disparition. Les
 * use-cases signalent leurs erreurs/succès ici.
 */
@Injectable({ providedIn: 'root' })
export class ToastStates {
  private readonly _messages = signal<IToast[]>([]);
  readonly messages = this._messages.asReadonly();

  private readonly autoDismissMs = 5000;

  addToast(toast: IToast): void {
    const id = (this._messages().at(-1)?.id ?? 0) + 1;
    const entry: IToast = { ...toast, id };
    this._messages.update((list) => [...list, entry]);
    setTimeout(() => this.removeToast(id), this.autoDismissMs);
  }

  removeToast(id: number): void {
    this._messages.update((list) => list.filter((t) => t.id !== id));
  }

  success(message: string): void {
    this.addToast({ message, messageType: ToastType.success, id: 0 });
  }

  error(message: string): void {
    this.addToast({ message, messageType: ToastType.error, id: 0 });
  }

  info(message: string): void {
    this.addToast({ message, messageType: ToastType.info, id: 0 });
  }

  warning(message: string): void {
    this.addToast({ message, messageType: ToastType.warning, id: 0 });
  }
}
