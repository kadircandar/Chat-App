export interface IncomingMessage {
  senderUserId: string;
  receiverUserId: string;
  content: string;
  isRead: boolean;
  sentAt: Date;
}

export interface PendingResponse {
  senderUsername: string;
  senderUserId: string;
  initials: string;
}

export interface Message {
  id?: string;
  senderUserId: string;
  receiverUserId: string;
  content: string;
  sentAt: Date;
  isRead: boolean;
}

export interface Friend {
  userId: string;
  username: string;
  unreadCount: number;
  initials: string;
  lastMessage: string;
  time: Date;
  typing: boolean;
  isOnline: boolean;
  senderUserId: string;
}

export interface SendMessageRequest {
  senderUserId: string;
  receiverUserId: string;
  content: string;
  isRead: boolean;
}

