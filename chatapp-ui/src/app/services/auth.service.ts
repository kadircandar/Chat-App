import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = environment.apiUrl + '/auth';
  private tokenKey = 'auth_token';
  private usernameKey = 'username';
  public isLoggedIn = new BehaviorSubject<boolean>(!!this.getToken());

  constructor(private http: HttpClient) {}

  register(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, {
      userName: username,
      password: password,
    });
  }

  login(username: string, password: string): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/login`, {
        username: username,
        password: password,
      })
      .pipe(
        tap((res: any) => {
          localStorage.setItem(this.tokenKey, res.token);
          localStorage.setItem(this.usernameKey, username);
          this.isLoggedIn.next(true);
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.usernameKey);
    this.isLoggedIn.next(false);
  }

  getToken() {
    return localStorage.getItem(this.tokenKey);
  }

  getUsername() {
    return localStorage.getItem(this.usernameKey);
  }

  getUserId() {
    return this.getUserIdFromToken(this.getToken() || '') || '';
  }

  private getUserIdFromToken(token: string): string | null {
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);
      return (
        decoded[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
        ] || null
      ); // userId alanı
    } catch (error) {
      console.error('Token decode error', error);
      return null;
    }
  }

  isTokenExpired(token: string): boolean {
    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Math.floor(Date.now() / 1000);
      return decoded.exp < currentTime;
    } catch {
      return true; // geçersiz token
    }
  }
}
