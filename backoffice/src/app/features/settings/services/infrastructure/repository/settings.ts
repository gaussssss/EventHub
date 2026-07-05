import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { GamificationRequest, GamificationSettingsDto } from '../../../models/gamificationSettingsDto';

/** Accès HTTP aux réglages admin (routes /api/admin/settings). */
@Injectable({ providedIn: 'root' })
export class SettingsService {
  private apiUrl = `${environment.apiUrl}/admin/settings`;

  constructor(private http: HttpClient) {}

  getGamification(): Observable<GamificationSettingsDto> {
    return this.http.get<GamificationSettingsDto>(`${this.apiUrl}/gamification`);
  }

  updateGamification(body: GamificationRequest): Observable<GamificationSettingsDto> {
    return this.http.patch<GamificationSettingsDto>(`${this.apiUrl}/gamification`, body);
  }
}
