import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { ReportDto } from '../../../models/reportDto';

/** Accès HTTP à la modération (routes /api/admin). */
@Injectable({ providedIn: 'root' })
export class ModerationService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  /** GET /admin/reports — file des signalements ouverts. */
  getReports(): Observable<ReportDto[]> {
    return this.http.get<ReportDto[]>(`${this.apiUrl}/reports`);
  }

  /** POST /admin/posts/{id}/hide — masquer une publication. */
  hidePost(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/posts/${id}/hide`, {});
  }

  /** POST /admin/comments/{id}/hide — masquer un commentaire. */
  hideComment(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/comments/${id}/hide`, {});
  }
}
