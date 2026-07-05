import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { BroadcastRequest } from '../../../models/broadcastRequest';

/** Accès HTTP aux notifications admin (routes /api/admin/notifications). */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/admin/notifications`;

  constructor(private http: HttpClient) {}

  /** POST /admin/notifications/broadcast → nombre de destinataires. */
  broadcast(body: BroadcastRequest): Observable<{ recipients: number }> {
    return this.http.post<{ recipients: number }>(`${this.apiUrl}/broadcast`, body);
  }
}
