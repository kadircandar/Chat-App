import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { Message, SendMessageRequest } from 'src/app/models/model';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private apiUrl = environment.apiUrl + '/messages';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    var token = this.authService.getToken() || '';
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  getMessageHistory(selectedFriend: string): Observable<Message[]> {
    return this.http.get<Message[]>(
      `${this.apiUrl}/history/${selectedFriend}`,
      {
        headers: this.getHeaders(),
      }
    );
  }

  sendMessage(message: SendMessageRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}`, message, {
      headers: this.getHeaders(),
    });
  }

  markAsRead(senderUserId: string): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/markasread`,
      {
        senderUserId: senderUserId,
      },
      { headers: this.getHeaders() }
    );
  }
}
