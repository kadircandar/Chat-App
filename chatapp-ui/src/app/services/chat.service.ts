import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { IncomingMessage } from '../models/model';

export const hubEvents = {
  ReceiveMessage: 'ReceiveMessage',
  UpdateOnlineFriends: 'UpdateOnlineFriends',
  UserDisconnected: 'UserDisconnected',
  SendMessage: 'SendMessage',
  PendingRequestsUpdated: 'PendingRequestsUpdated',
  FriendListUpdated: 'FriendListUpdated',
};

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private hubConnection!: signalR.HubConnection;

  private messages = new BehaviorSubject<IncomingMessage[]>([]);
  messages$ = this.messages.asObservable();

  private onlineFriends = new BehaviorSubject<string[]>([]);
  onlineFriends$ = this.onlineFriends.asObservable();

  constructor() {}

  // Belirli bir event için abonelik
  public on(eventName: string, callback: (data: any) => void): void {
    if (this.hubConnection) {
      this.hubConnection.on(eventName, callback);
    }
  }

  public listenToEvent(
    eventName: string,
    callback: (receiver: any, message: any) => void
  ): void {
    if (this.hubConnection) {
      this.hubConnection.on(eventName, callback);
    }
  }

  startConnection(token: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.baseUrl + '/chathub', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connected'))
      .catch((err) => console.error('SignalR Connection Error:', err));

    // Mesaj geldiğinde "ReceiveMessage" olayını dinle
    this.hubConnection.on(
      hubEvents.ReceiveMessage,
      (message: IncomingMessage) => {
        const current = this.messages.value;
        current.push({
          content: message.content,
          senderUserId: message.senderUserId,
          receiverUserId: message.receiverUserId,
          isRead: message.isRead,
          sentAt: message.sentAt,
        });
        this.messages.next(current);
      }
    );

    // Online arkadaşlar güncellendiğinde "UpdateOnlineFriends" olayını dinle
    this.hubConnection.on(hubEvents.UpdateOnlineFriends, (onlineUsers: string[]) => {
      this.onlineFriends.next(onlineUsers);
    });

    // Backend'den gelen "UserDisconnected" olayını dinle
    this.hubConnection.on(hubEvents.UserDisconnected, (userId: string) => {
      const updated = this.onlineFriends.value.filter((u) => u !== userId);
      this.onlineFriends.next(updated);
    });
  }

  sendMessage(receiverUserId: string, message: string) {
    this.hubConnection
      .invoke(hubEvents.SendMessage, receiverUserId, message)
      .catch((err) => console.error(err));
  }

  stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => {
        console.log('SignalR Disconnected');
      });
    }
  }
}
