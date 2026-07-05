import { Injectable, signal } from '@angular/core';

/** État d'interface transverse (repli du rail de navigation, etc.). */
@Injectable({ providedIn: 'root' })
export class UiStates {
  private readonly _sidebarOpen = signal(true);
  readonly sidebarOpen = this._sidebarOpen.asReadonly();

  toggleSidebar(): void {
    this._sidebarOpen.update((open) => !open);
  }
}
