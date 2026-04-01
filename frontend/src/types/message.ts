export const AI_BOT_USER_ID = 'mamvibe-ai-assistant';

export interface Message {
  id: string;
  senderId: string;
  senderDisplayName: string;
  senderAvatarUrl: string | null;
  receiverId: string;
  content: string;
  timestamp: string;
  isRead: boolean;
}

export interface Conversation {
  userId: string;
  displayName: string;
  avatarUrl: string | null;
  lastMessage: string;
  lastMessageTime: string;
  unreadCount: number;
}
