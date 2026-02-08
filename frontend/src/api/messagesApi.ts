import axiosClient from './axiosClient';
import type { Message, Conversation } from '../types/message';

export const messagesApi = {
  getConversations: () =>
    axiosClient.get<Conversation[]>('/messages/conversations'),

  getMessages: (userId: string) =>
    axiosClient.get<Message[]>(`/messages/${userId}`),

  send: (receiverId: string, content: string) =>
    axiosClient.post<Message>('/messages', { receiverId, content }),

  markAsRead: (userId: string) =>
    axiosClient.put(`/messages/${userId}/read`),
};
