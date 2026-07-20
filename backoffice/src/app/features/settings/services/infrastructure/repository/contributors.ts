import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { ContributorDto, ContributorRequest } from '../../../models/contributorDto';

/** Accès HTTP aux contributeurs de la page « À propos » (/api/admin/contributors). */
@Injectable({ providedIn: 'root' })
export class ContributorsService {
  private apiUrl = `${environment.apiUrl}/admin/contributors`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ContributorDto[]> {
    return this.http.get<ContributorDto[]>(this.apiUrl);
  }

  create(body: ContributorRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, body);
  }

  update(id: string, body: ContributorRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
