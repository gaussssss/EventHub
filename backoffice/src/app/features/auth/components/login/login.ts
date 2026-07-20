import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';

/** Page de connexion : bouton « Se connecter avec Microsoft » (MSAL redirection). */
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
    // Au retour de la redirection Microsoft, `initialize()` a déjà restauré le
    // compte : si authentifié, on file au tableau de bord.
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
      // Redirection pleine page : en cas de succès la page se décharge (la
      // navigation vers /dashboard se fait au retour via ngOnInit).
      await this.auth.login();
    } catch {
      this.toasts.error('Échec de la connexion Microsoft.');
      this.isLoading.set(false);
    }
  }
}
