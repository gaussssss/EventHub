import { Component, input, output } from '@angular/core';
import { IToast } from '../../models/interfaces/IToast';

/** Toast présentationnel : reçoit un message et émet sa fermeture. */
@Component({
  selector: 'app-toast',
  standalone: true,
  templateUrl: './toast.html',
})
export class Toast {
  readonly toast = input.required<IToast>();
  readonly closed = output<number>();

  icon(): string {
    const map: Record<string, string> = {
      success: 'icon-[fluent--checkmark-circle-24-filled]',
      error: 'icon-[fluent--dismiss-circle-24-filled]',
      warning: 'icon-[fluent--warning-24-filled]',
      info: 'icon-[fluent--info-24-filled]',
    };
    return map[this.toast().messageType] ?? 'icon-[fluent--info-24-filled]';
  }

  alertClass(): string {
    const map: Record<string, string> = {
      success: 'alert-success',
      error: 'alert-error',
      warning: 'alert-warning',
      info: 'alert-info',
    };
    return map[this.toast().messageType] ?? 'alert-info';
  }
}
