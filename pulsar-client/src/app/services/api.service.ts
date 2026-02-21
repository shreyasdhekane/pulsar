import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getFeaturedEndpoints(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/api/endpoints/featured`);
  }
  getStats(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/api/endpoints/stats`);
  }
  addCustomEndpoint(name: string, url: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/api/endpoints/custom`, { name, url });
  }
  getEndpointDetail(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/api/endpoints/${id}`);
  }
}
