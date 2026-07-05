import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SendBroadcast } from '../../services/application/sendBroadcast';

/** Écran « Notifications » : diffuser une notification push à une audience. */
@Component({
  selector: 'app-notification-broadcast',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './notification-broadcast.html',
})
export class NotificationBroadcast {
  readonly send = inject(SendBroadcast);

  readonly audience = signal('all');
  readonly title = signal('');
  readonly body = signal('');

  submit(): void {
    this.send.handler(
      { audience: this.audience(), title: this.title().trim(), body: this.body().trim() },
      () => {
        this.title.set('');
        this.body.set('');
      },
    );
  }
}
