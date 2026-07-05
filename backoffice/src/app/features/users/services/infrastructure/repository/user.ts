import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { AdjustHeartsRequest, AdjustHeartsResult } from '../../../models/adjustHeartsRequest';
import { UpdateUserRequest } from '../../../models/updateUserRequest';
import { UserDto } from '../../../models/userDto';

/** Accès HTTP à l'administration des utilisateurs (routes /api/admin/users). */
@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = `${environment.apiUrl}/admin/users`;

  constructor(private http: HttpClient) {}

  /** GET /admin/users?q= — recherche (q optionnel). */
  search(q?: string): Observable<UserDto[]> {
    let params = new HttpParams();
    if (q) params = params.set('q', q);
    return this.http.get<UserDto[]>(this.apiUrl, { params });
  }

  /** PATCH /admin/users/{id} — change rôle et/ou statut (204). */
  update(id: string, request: UpdateUserRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}`, request);
  }

  /** POST /admin/users/{id}/hearts — ajuste les cœurs → nouveau total. */
  adjustHearts(id: string, request: AdjustHeartsRequest): Observable<AdjustHeartsResult> {
    return this.http.post<AdjustHeartsResult>(`${this.apiUrl}/${id}/hearts`, request);
  }
}
