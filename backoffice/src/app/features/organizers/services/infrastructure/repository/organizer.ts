import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { OrganizerDto, OrganizerRequest } from '../../../models/organizerDto';

/** Accès HTTP au CRUD des organisateurs (routes /api/admin/organizers). */
@Injectable({ providedIn: 'root' })
export class OrganizerService {
  private apiUrl = `${environment.apiUrl}/admin/organizers`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<OrganizerDto[]> {
    return this.http.get<OrganizerDto[]>(this.apiUrl);
  }

  create(request: OrganizerRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  update(id: string, request: OrganizerRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
