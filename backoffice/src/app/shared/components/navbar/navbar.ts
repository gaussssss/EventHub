import { Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { ThemeService } from '../../../core/services/theme';
import { UiStates } from '../../services/infrastructure/states/uiStates';

/** Barre supérieure (bleu Azure) : bascule du rail + thème + utilisateur. */
@Component({
  selector: 'app-navbar',
  standalone: true,
  templateUrl: './navbar.html',
})
export class Navbar {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly ui = inject(UiStates);
  readonly theme = inject(ThemeService);

  /** Classe d'icône du bouton thème (lune en clair, soleil en sombre). */
  readonly themeIcon = computed(() =>
    this.theme.isDark()
      ? 'icon-[fluent--weather-sunny-24-regular]'
      : 'icon-[fluent--weather-moon-24-regular]',
  );

  readonly account = this.auth.account;

  readonly name = computed(
    () => this.account()?.name ?? this.account()?.username ?? 'Utilisateur',
  );

  readonly initials = computed(() => {
    const n = this.name().trim();
    if (!n) return '?';
    const parts = n.split(/\s+/);
    return (parts[0]![0]! + (parts[1]?.[0] ?? '')).toUpperCase();
  });

  async signOut(): Promise<void> {
    await this.auth.logout();
    this.router.navigate(['/login']);
  }
}
