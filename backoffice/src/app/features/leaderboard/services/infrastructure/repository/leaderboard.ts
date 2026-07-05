import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { LeaderboardRow } from '../../../models/leaderboardRow';

/** Accès HTTP au classement global (GET /api/leaderboard). */
@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private apiUrl = `${environment.apiUrl}/leaderboard`;

  constructor(private http: HttpClient) {}

  getLeaderboard(page: number): Observable<LeaderboardRow[]> {
    const params = new HttpParams().set('page', page);
    return this.http.get<LeaderboardRow[]>(this.apiUrl, { params });
  }
}
