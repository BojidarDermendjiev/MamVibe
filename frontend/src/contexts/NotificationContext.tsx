import { createContext, useContext, useEffect, useState, useCallback, useRef, type ReactNode } from 'react';
import { useSignalR } from './SignalRContext';
import { useAuthStore } from '../store/authStore';
import { messagesApi } from '../api/messagesApi';

interface NotificationContextValue {
  unreadCount: number;
  markConversationRead: (userId: string) => void;
  setActiveChatUserId: (userId: string | null) => void;
}

const NotificationContext = createContext<NotificationContextValue>({
  unreadCount: 0,
  markConversationRead: () => {},
  setActiveChatUserId: () => {},
});

export function NotificationProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated, user } = useAuthStore();
  const { onMessage } = useSignalR();
  const [unreadCount, setUnreadCount] = useState(0);
  const activeChatRef = useRef<string | null>(null);

  // Load initial unread count from conversations
  useEffect(() => {
    if (!isAuthenticated) {
      setUnreadCount(0);
      return;
    }

    messagesApi
      .getConversations()
      .then((res) => {
        const total = res.data.reduce((sum, c) => sum + c.unreadCount, 0);
        setUnreadCount(total);
      })
      .catch(() => {});
  }, [isAuthenticated]);

  // Increment unread count on new messages from others,
  // but NOT if the message is from the currently active chat
  useEffect(() => {
    const unsub = onMessage((msg) => {
      const isFromActiveChat = msg.senderId === activeChatRef.current;
      const isFromMe = msg.senderId === user?.id;

      if (!isFromActiveChat && !isFromMe) {
        setUnreadCount((prev) => prev + 1);
      }
    });
    return unsub;
  }, [onMessage, user?.id]);

  const setActiveChatUserId = useCallback((userId: string | null) => {
    activeChatRef.current = userId;
  }, []);

  const markConversationRead = useCallback((userId: string) => {
    messagesApi.markAsRead(userId).catch(() => {});
    // Re-fetch to get accurate count
    if (isAuthenticated) {
      messagesApi
        .getConversations()
        .then((res) => {
          const total = res.data.reduce((sum, c) => sum + c.unreadCount, 0);
          setUnreadCount(total);
        })
        .catch(() => {});
    }
  }, [isAuthenticated]);

  return (
    <NotificationContext.Provider value={{ unreadCount, markConversationRead, setActiveChatUserId }}>
      {children}
    </NotificationContext.Provider>
  );
}

export function useNotification(): NotificationContextValue {
  return useContext(NotificationContext);
}
