import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';

/**
 * Téléversement d'images vers l'API (POST /api/uploads/image, multipart
 * « file »). L'API stocke le fichier et renvoie un chemin **relatif**
 * (`/uploads/…`), jamais de domaine, que l'on enregistre tel quel comme
 * `imageUrl` de l'activité (les clients le résolvent contre leur base API).
 */
@Injectable({ providedIn: 'root' })
export class UploadsService {
  private apiUrl = `${environment.apiUrl}/uploads/image`;

  constructor(private http: HttpClient) {}

  uploadImage(file: File): Observable<{ url: string }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ url: string }>(this.apiUrl, form);
  }
}
