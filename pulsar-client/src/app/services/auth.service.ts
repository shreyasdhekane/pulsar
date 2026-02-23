import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = environment.apiUrl;

  isLoggedIn = signal(false);
  displayName = signal('');

  constructor(private http: HttpClient) {
    const token = localStorage.getItem('token');
    if (token) {
      this.isLoggedIn.set(true);
      this.displayName.set(localStorage.getItem('displayName') || '');
    }
  }

  login(email: string, password: string) {
    return this.http.post<any>(`${this.baseUrl}/api/auth/login`, { email, password });
  }

  register(email: string, password: string) {
    return this.http.post<any>(`${this.baseUrl}/api/auth/register`, { email, password });
  }

  saveSession(token: string, displayName: string) {
    localStorage.setItem('token', token);
    localStorage.setItem('displayName', displayName);
    this.isLoggedIn.set(true);
    this.displayName.set(displayName);
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('displayName');
    this.isLoggedIn.set(false);
    this.displayName.set('');
  }

  getToken() {
    return localStorage.getItem('token');
  }
}
