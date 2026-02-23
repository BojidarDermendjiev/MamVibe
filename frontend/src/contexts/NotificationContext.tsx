import { createContext, useContext, useEffect, useState, useCallback, useRef, type ReactNode } from 'react';
import { useSignalR } from './SignalRContext';
import { useAuthStore } from '../store/authStore';
import { messagesApi } from '../api/messagesApi';
import { purchaseRequestsApi } from '../api/purchaseRequestsApi';
import { PurchaseRequestStatus } from '../types/purchaseRequest';

interface NotificationContextValue {
  unreadCount: number;
  pendingRequestCount: number;
  markConversationRead: (userId: string) => void;
  setActiveChatUserId: (userId: string | null) => void;
  decrementPendingRequestCount: () => void;
}

const NotificationContext = createContext<NotificationContextValue>({
  unreadCount: 0,
  pendingRequestCount: 0,
  markConversationRead: () => {},
  setActiveChatUserId: () => {},
  decrementPendingRequestCount: () => {},
});

export function NotificationProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated, user } = useAuthStore();
  const { onMessage, onPurchaseRequest } = useSignalR();
  const [unreadCount, setUnreadCount] = useState(0);
  const [pendingRequestCount, setPendingRequestCount] = useState(0);
  const activeChatRef = useRef<string | null>(null);

  // Reset counts on logout; fetch initial counts on login
  useEffect(() => {
    if (!isAuthenticated) {
      // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional state reset on auth change
      setUnreadCount(0);
      // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional state reset on auth change
      setPendingRequestCount(0);
      return;
    }

    let cancelled = false;

    messagesApi
      .getConversations()
      .then((res) => {
        if (!cancelled) {
          const total = res.data.reduce((sum, c) => sum + c.unreadCount, 0);
          setUnreadCount(total);
        }
      })
      .catch(() => {});

    purchaseRequestsApi
      .getAsSeller()
      .then((res) => {
        if (!cancelled) {
          const pending = res.data.filter((r) => r.status === PurchaseRequestStatus.Pending).length;
          setPendingRequestCount(pending);
        }
      })
      .catch(() => {});

    return () => { cancelled = true; };
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

  // Increment pending request count when seller receives a new request
  useEffect(() => {
    const unsub = onPurchaseRequest(() => {
      setPendingRequestCount((prev) => prev + 1);
    });
    return unsub;
  }, [onPurchaseRequest]);

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

  const decrementPendingRequestCount = useCallback(() => {
    setPendingRequestCount((prev) => Math.max(0, prev - 1));
  }, []);

  return (
    <NotificationContext.Provider
      value={{ unreadCount, pendingRequestCount, markConversationRead, setActiveChatUserId, decrementPendingRequestCount }}
    >
      {children}
    </NotificationContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useNotification(): NotificationContextValue {
  return useContext(NotificationContext);
}
