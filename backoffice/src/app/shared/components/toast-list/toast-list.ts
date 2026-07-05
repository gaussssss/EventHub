import { Component, inject } from '@angular/core';
import { ToastStates } from '../../services/infrastructure/states/toastStates';
import { Toast } from '../toast/toast';

/** Pile de toasts (coin bas-droit), branchée sur `ToastStates`. */
@Component({
  selector: 'app-toast-list',
  standalone: true,
  imports: [Toast],
  templateUrl: './toast-list.html',
})
export class ToastList {
  readonly toasts = inject(ToastStates);
}
