import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from, switchMap } from 'rxjs';
import { AuthService } from '../services/auth';

/**
 * Injecte `Authorization: Bearer <access token Entra>` sur chaque requête API.
 * Le token est obtenu (silencieusement) via MSAL ; si absent, la requête part
 * telle quelle (l'API répondra 401, géré par les use-cases → toast).
 */
export const auth: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  return from(authService.getToken()).pipe(
    switchMap((token) => {
      const request = token
        ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
        : req;
      return next(request);
    }),
  );
};
