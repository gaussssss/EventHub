import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';

/** Page de connexion : bouton « Se connecter avec Microsoft » (MSAL popup). */
@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.html',
})
export class Login implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toasts = inject(ToastStates);

  readonly isLoading = signal(false);
  readonly isConfigured = this.auth.isConfigured;

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) this.router.navigate(['/dashboard']);
  }

  async signIn(): Promise<void> {
    if (!this.auth.isConfigured) {
      this.toasts.error(
        "Microsoft Entra n'est pas configuré : renseigner le clientId de l'app SPA.",
      );
      return;
    }

    this.isLoading.set(true);
    try {
      await this.auth.login();
      this.router.navigate(['/dashboard']);
    } catch {
      this.toasts.error('Échec de la connexion Microsoft.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
