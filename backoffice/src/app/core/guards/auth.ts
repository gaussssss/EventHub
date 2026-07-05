import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

/**
 * Protège les routes du back-office : laisse passer si une session Entra est
 * ouverte, sinon redirige vers `/login`. (MSAL est déjà initialisé au démarrage
 * via `provideAppInitializer`, donc la session en cache est restaurée ici.)
 */
export const auth: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) return true;

  router.navigate(['/login']);
  return false;
};
