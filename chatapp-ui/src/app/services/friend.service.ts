import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class FriendService {
  private apiUrl = environment.apiUrl + '/friends';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    var token = this.authService.getToken() || '';
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  getFriends(): Observable<any> {
    return this.http.get(`${this.apiUrl}/list`, { headers: this.getHeaders() });
  }

  getPendingRequests(): Observable<any> {
    return this.http.get(`${this.apiUrl}/requests/pending`, {
      headers: this.getHeaders(),
    });
  }

  sendFriendRequest(username: string): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/requests`,
      { friendUsername: username },
      { headers: this.getHeaders() }
    );
  }

  respondFriendRequest(
    senderUsername: string,
    accept: boolean
  ): Observable<any> {
    return this.http.patch(
      `${this.apiUrl}/requests/${senderUsername}?accept=${accept}`,
      {},
      { headers: this.getHeaders() }
    );
  }
}
