import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { AdminActivityDto } from '../../../models/adminActivityDto';
import { ActivityDashboardDto } from '../../../models/activityDashboardDto';
import { ActivityDetailDto } from '../../../models/activityDetailDto';
import { ActivityRequest } from '../../../models/activityRequest';
import { RegistrationEntryDto } from '../../../models/registrationEntryDto';

/** Accès HTTP à l'administration des activités (routes /api/admin/activities). */
@Injectable({ providedIn: 'root' })
export class ActivityService {
  private apiUrl = `${environment.apiUrl}/admin/activities`;

  constructor(private http: HttpClient) {}

  /** GET /admin/activities — toutes les activités (tous statuts). */
  getAll(): Observable<AdminActivityDto[]> {
    return this.http.get<AdminActivityDto[]>(this.apiUrl);
  }

  /** GET /admin/activities/{id} — détail complet (édition). */
  getById(id: string): Observable<ActivityDetailDto> {
    return this.http.get<ActivityDetailDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /admin/activities — créer une activité. */
  create(body: ActivityRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, body);
  }

  /** PUT /admin/activities/{id} — mettre à jour une activité. */
  update(id: string, body: ActivityRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, body);
  }

  /** POST /admin/activities/{id}/publish. */
  publish(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/publish`, {});
  }

  /** POST /admin/activities/{id}/cancel. */
  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/cancel`, {});
  }

  /** POST /admin/activities/{id}/feature → nouvel état « à la une ». */
  feature(id: string): Observable<{ isFeatured: boolean }> {
    return this.http.post<{ isFeatured: boolean }>(`${this.apiUrl}/${id}/feature`, {});
  }

  /** GET /admin/activities/{id}/registrations — inscrits + liste d'attente. */
  getRegistrations(id: string): Observable<RegistrationEntryDto[]> {
    return this.http.get<RegistrationEntryDto[]>(`${this.apiUrl}/${id}/registrations`);
  }

  /** POST /admin/activities/{id}/attendance — marque présents + crédite les cœurs. */
  markAttendance(id: string, userIds: string[]): Observable<{ credited: number }> {
    return this.http.post<{ credited: number }>(`${this.apiUrl}/${id}/attendance`, { userIds });
  }

  /** GET /admin/dashboard/activities/{id} — statistiques de l'activité. */
  getDashboard(id: string): Observable<ActivityDashboardDto> {
    return this.http.get<ActivityDashboardDto>(
      `${environment.apiUrl}/admin/dashboard/activities/${id}`,
    );
  }
}
