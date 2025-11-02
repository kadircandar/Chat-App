import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import {
  PendingResponse,
  Friend,
  Message,
  SendMessageRequest,
  IncomingMessage,
} from 'src/app/models/model';
import { AuthService } from 'src/app/services/auth.service';
import { ChatService, hubEvents } from 'src/app/services/chat.service';
import { FriendService } from 'src/app/services/friend.service';
import { MessageService } from 'src/app/services/message.service';
import { ToastService } from 'src/app/services/toast.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css'],
})
export class ChatComponent implements OnInit {
  activeTab: 'contacts' | 'requests' = 'contacts';
  selectedUser: { username: string; userId: string } = {
    username: '',
    userId: '',
  };
  currentUser: { username: string; userId: string } = {
    username: '',
    userId: '',
  };
  searchText: string = '';
  activeIndex = -1;
  message = '';

  messages: Message[] = [];
  friends: Friend[] = [];
  friendsTempData: Friend[] = [];
  pendingRequests: PendingResponse[] = [];

  constructor(
    private chatService: ChatService,
    private authService: AuthService,
    private router: Router,
    private friendService: FriendService,
    private messageService: MessageService,
    private toastService: ToastService
  ) {}

  ngOnDestroy(): void {
    this.chatService.stopConnection();
  }

  async ngOnInit() {
    this.currentUser = {
      username: this.authService.getUsername() || '',
      userId: this.authService.getUserId() || '',
    };

    this.chatService.startConnection(this.authService.getToken() || '');

    await this.loadFriends();
    await this.loadPendingRequests();

    this.updateOnlineFriends();
    this.updateMessages();
    this.listenToPendingRequests();
    this.listenToFriendUpdated();
  }

  updateMessages() {
    this.chatService.on(hubEvents.ReceiveMessage, (message: IncomingMessage) => {
      // Mesaj geldiğinde "ReceiveMessage" olayını dinle
      if (this.selectedUser.userId === message.senderUserId) {
        message.isRead = true;
      }
      this.updateUnreadBadge(message);
      this.messages.push(message);
    });
  }

  updateOnlineFriends() {
    // SignalR online arkadaşlar
    this.chatService.onlineFriends$.subscribe((onlineUserIds) => {
      this.friends.forEach((f) => {
        f.isOnline = onlineUserIds.includes(f.userId);
      });
    });
  }

  logout(): void {
    this.currentUser = { username: '', userId: '' };
    this.chatService.stopConnection();
    this.updateOnlineFriends();
    this.authService.logout();
    this.router.navigate(['/auth']);
  }

  async loadFriends() {
    this.friends = [];
    const res = await firstValueFrom(this.friendService.getFriends());
    this.friends = res;
    this.friendsTempData = res;
  }

  async loadPendingRequests() {
    this.pendingRequests = [];
    const res = await firstValueFrom(this.friendService.getPendingRequests());
    this.pendingRequests = res;
  }

  listenToPendingRequests(): void {
    this.chatService.on(hubEvents.PendingRequestsUpdated, (data: PendingResponse) => {
      this.pendingRequests.push(data);
    });
  }

  listenToFriendUpdated(): void {
    this.chatService.on(hubEvents.FriendListUpdated, (data: Friend) => {
      this.friends.push(data);
    });
  }

  loadMessageHistory(selectedUserId: string): void {
    this.messages = [];
    this.messageService.getMessageHistory(selectedUserId).subscribe((res) => {
      this.messages = res;

      var unreadCount = this.messages.filter(
        (f) => !f.isRead && f.senderUserId === selectedUserId
      ).length;

      if (this.messages.length === 0 || unreadCount === 0) return;

      this.messageService.markAsRead(selectedUserId).subscribe({
        next: () => {
          this.markMessagesAsRead();
          this.resetUnreadBadge(selectedUserId);
        },
        error: (err) => {
          this.toastService.showError(err.error);
        },
      });
    });
  }

  sendMessage(): void {
    if (!this.selectedUser.username || !this.message) return;

    const friend = this.friends.find(
      (f) => f.userId === this.selectedUser.userId
    );
    friend!.lastMessage = this.message;
    friend!.senderUserId = this.currentUser.userId;

    this.sendeMessageDb();
  }

  sendeMessageDb() {
    if (!this.selectedUser.username || !this.message) return;

    const message: SendMessageRequest = {
      senderUserId: this.currentUser.userId,
      receiverUserId: this.selectedUser.userId,
      content: this.message,
      isRead: false,
    };
    this.messageService.sendMessage(message).subscribe({
      next: () => {
        this.messages.push({
          senderUserId: this.currentUser.userId,
          receiverUserId: this.selectedUser.userId,
          content: this.message,
          sentAt: new Date(),
          isRead: false,
        });

        this.message = '';
      },
      error: (err) => {
        this.toastService.showError(err.error);
      },
    });
  }

  updateUnreadBadge(message: IncomingMessage) {
    const friend = this.friends.find((f) => f.userId === message.senderUserId);
    if (!friend) return;

    friend.senderUserId = message.senderUserId;
    friend.lastMessage = message.content;

    if (message.senderUserId !== this.selectedUser.userId) {
      friend.unreadCount += 1; // UI’de badge için kullanılır
    } else {
      friend.unreadCount = 0;
      this.messages
        .filter((m) => m.receiverUserId === message.senderUserId && !m.isRead)
        .forEach((m) => {
          m.isRead = true;
        });
    }
  }

  resetUnreadBadge(receiverUserId: string) {
    const friend = this.friends.find((f) => f.userId === receiverUserId);
    if (!friend) return;

    if (receiverUserId === this.selectedUser.userId) {
      friend.unreadCount = 0;
      this.messages
        .filter((m) => m.receiverUserId === receiverUserId && !m.isRead)
        .forEach((m) => {
          m.isRead = true;
        });
    }
  }

  markMessagesAsRead() {
    this.messages
      .filter((m) => !m.isRead)
      .forEach((m) => {
        m.isRead = true;
      });
  }

  openChat(selectedUsername: string, index: number, selectedUserId: string) {
    this.selectedUser = { username: selectedUsername, userId: selectedUserId };
    this.activeIndex = index;
    this.loadMessageHistory(selectedUserId);
  }

  closeChat() {
    this.selectedUser = { username: '', userId: '' };
    this.activeIndex = -1;
  }

  isOnlineUser(): string {
    return this.friends.some(
      (f) => f.userId == this.selectedUser.userId && f.isOnline
    )
      ? 'Çevrimiçi'
      : 'Çevrimdışı';
  }

  showAddFriendPopup() {
    Swal.fire({
      title: 'Kullanıcı adı girin',
      input: 'text',
      inputPlaceholder: 'Kullanıcı adı...',
      confirmButtonColor: '#25D366',
      showCancelButton: true,
      cancelButtonText: 'İptal',
      confirmButtonText: 'Gönder',
      preConfirm: (value) => {
        if (!value) {
          Swal.showValidationMessage('Bu alan boş geçilemez!');
        }
      },
    }).then((result) => {
      if (result.isConfirmed) {
        this.friendService.sendFriendRequest(result.value).subscribe({
          next: () => {
            this.toastService.showSuccess(
              'Arkadaşlık isteğin başarıyla gönderildi! 🎉'
            );
          },
          error: (err) => {
            this.toastService.showError(err.error);
          },
        });
      }
    });
  }

  acceptRequest(req: PendingResponse) {
    this.friendService
      .respondFriendRequest(req.senderUsername, true)
      .subscribe((res: Friend) => {
        this.friends.push(res);
        this.pendingRequests = this.pendingRequests.filter((r) => r !== req);
      });
  }

  rejectRequest(req: PendingResponse) {
    this.friendService
      .respondFriendRequest(req.senderUsername, false)
      .subscribe(() => {
        this.loadFriends();
        this.loadPendingRequests();
        this.pendingRequests = this.pendingRequests.filter((r) => r !== req);
      });
  }

  filterItems() {
    if (!this.searchText) {
      this.friends = [...this.friendsTempData];
      return;
    }
    this.friends = this.friends.filter((item) =>
      item.username.toLowerCase().includes(this.searchText.toLowerCase())
    );
  }
}
