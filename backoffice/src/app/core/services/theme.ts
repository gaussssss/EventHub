import { Injectable, signal } from '@angular/core';

/**
 * Thème clair / sombre. Applique le thème daisyUI (`azure` / `azure-dark`) via
 * l'attribut `data-theme` sur `<html>`, persiste le choix dans `localStorage`,
 * et retombe sur la préférence système au premier lancement.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'eventhub-theme';
  private readonly _dark = signal<boolean>(this.initialDark());
  readonly isDark = this._dark.asReadonly();

  constructor() {
    this.apply();
  }

  toggle(): void {
    this._dark.update((d) => !d);
    try {
      localStorage.setItem(this.storageKey, this._dark() ? 'dark' : 'light');
    } catch {
      /* localStorage indisponible : on ignore, le choix ne sera pas persisté. */
    }
    this.apply();
  }

  private initialDark(): boolean {
    try {
      const saved = localStorage.getItem(this.storageKey);
      if (saved === 'dark') return true;
      if (saved === 'light') return false;
    } catch {
      /* ignore */
    }
    return (
      typeof window !== 'undefined' &&
      window.matchMedia?.('(prefers-color-scheme: dark)').matches === true
    );
  }

  private apply(): void {
    document.documentElement.setAttribute('data-theme', this._dark() ? 'azure-dark' : 'azure');
  }
}
