import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { SeedResult } from '../../../models/seedResult';

/** Outils de développement (routes /api/admin/dev, dev-only côté API). */
@Injectable({ providedIn: 'root' })
export class DevService {
  private apiUrl = `${environment.apiUrl}/admin/dev`;

  constructor(private http: HttpClient) {}

  /** POST /admin/dev/seed — réinitialise et régénère les données de démo. */
  seed(): Observable<SeedResult> {
    return this.http.post<SeedResult>(`${this.apiUrl}/seed`, {});
  }
}
