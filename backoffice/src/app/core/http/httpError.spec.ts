import { HttpErrorResponse } from '@angular/common/http';
import { httpErrorMessage } from './httpError';

describe('httpErrorMessage', () => {
  it('lit le detail d\'un ProblemDetails', () => {
    const err = new HttpErrorResponse({
      status: 400,
      error: { title: 'Requête invalide', detail: 'maxParticipants doit être strictement positif.' },
    });
    expect(httpErrorMessage(err)).toBe('maxParticipants doit être strictement positif.');
  });

  it('lit le champ { error } d\'un BadRequest maison', () => {
    const err = new HttpErrorResponse({ status: 409, error: { error: 'slug déjà utilisé' } });
    expect(httpErrorMessage(err)).toBe('slug déjà utilisé');
  });

  it('retombe sur title si ni detail ni error', () => {
    const err = new HttpErrorResponse({ status: 400, error: { title: 'Requête invalide' } });
    expect(httpErrorMessage(err)).toBe('Requête invalide');
  });

  it('gère un corps en chaîne brute', () => {
    const err = new HttpErrorResponse({ status: 500, error: 'boom' });
    expect(httpErrorMessage(err)).toBe('boom');
  });

  it('renvoie null quand rien n\'est exploitable', () => {
    const err = new HttpErrorResponse({ status: 0, error: null });
    expect(httpErrorMessage(err)).toBeNull();
  });
});
