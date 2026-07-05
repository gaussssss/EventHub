import { HttpErrorResponse } from '@angular/common/http';

/**
 * Extrait le message d'erreur exploitable d'une réponse d'API.
 * - ProblemDetails (RFC 7807, invariants de domaine) → `detail`.
 * - BadRequest/Conflict maison → `{ error }`.
 * - Repli sur `title` ou une chaîne brute.
 * Renvoie `null` si rien d'exploitable (l'appelant met alors un message par défaut).
 */
export function httpErrorMessage(err: HttpErrorResponse): string | null {
  const body = err?.error;

  if (body && typeof body === 'object') {
    const candidate = body.detail ?? body.error ?? body.title;
    if (typeof candidate === 'string' && candidate.trim()) return candidate;
  }

  if (typeof body === 'string' && body.trim()) return body;

  return null;
}
