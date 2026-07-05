import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { DashboardOverviewDto } from '../../../models/dashboardOverviewDto';

/** Accès HTTP au tableau de bord admin (routes /api/admin). */
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  /** GET /admin/dashboard/overview — KPIs. */
  getOverview(): Observable<DashboardOverviewDto> {
    return this.http.get<DashboardOverviewDto>(`${this.apiUrl}/dashboard/overview`);
  }

  /** GET /admin/exports/registrations.csv — fichier CSV (blob). */
  exportRegistrationsCsv(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/exports/registrations.csv`, { responseType: 'blob' });
  }
}
