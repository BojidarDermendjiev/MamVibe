import axiosClient from './axiosClient';

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export const assistantApi = {
  chat: (message: string, history: ChatMessage[], language: string) =>
    axiosClient.post<{ reply: string }>('/assistant/chat', { message, history, language }),
};
