import { createContext, useContext, useEffect, useState, useCallback, useRef, type ReactNode } from 'react';
import toast from 'react-hot-toast';
import { useSignalR } from './SignalRContext';
import { useAuthStore } from '../store/authStore';
import { messagesApi } from '../api/messagesApi';
import { purchaseRequestsApi } from '../api/purchaseRequestsApi';
import { PurchaseRequestStatus } from '../types/purchaseRequest';
import { CourierProvider } from '../types/shipping';

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
  const { onMessage, onPurchaseRequest, onShipmentCreated, onShipmentStatusChanged } = useSignalR();
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

  // Notify buyer when their shipment waybill is created (item has been shipped)
  useEffect(() => {
    const unsub = onShipmentCreated((shipment) => {
      const courierNames: Record<number, string> = {
        [CourierProvider.Econt]: 'Econt',
        [CourierProvider.Speedy]: 'Speedy',
        [CourierProvider.BoxNow]: 'Box Now',
      };
      const courier = courierNames[shipment.courierProvider] ?? '';
      toast(
        (t) => (
          <div className="flex flex-col gap-1">
            <p className="font-semibold text-primary">📦 Your order has been shipped!</p>
            <p className="text-sm text-gray-600">
              {shipment.itemTitle && <span>"{shipment.itemTitle}" </span>}
              via {courier}
              {shipment.trackingNumber && <span> · #{shipment.trackingNumber}</span>}
            </p>
            <a
              href={`/shipments/${shipment.id}`}
              onClick={() => toast.dismiss(t.id)}
              className="text-sm font-medium text-primary underline mt-1"
            >
              Track shipment →
            </a>
          </div>
        ),
        { duration: 10000 }
      );
    });
    return unsub;
  }, [onShipmentCreated]);

  // Notify buyer when courier picks up the package
  useEffect(() => {
    const unsub = onShipmentStatusChanged((shipment) => {
      const courierNames: Record<number, string> = {
        [CourierProvider.Econt]: 'Econt',
        [CourierProvider.Speedy]: 'Speedy',
        [CourierProvider.BoxNow]: 'Box Now',
      };
      const courier = courierNames[shipment.courierProvider] ?? '';
      toast(
        (t) => (
          <div className="flex flex-col gap-1">
            <p className="font-semibold text-primary">🚚 Package picked up!</p>
            <p className="text-sm text-gray-600">
              {shipment.itemTitle && <span>"{shipment.itemTitle}" </span>}
              is on its way via {courier}.
            </p>
            <a
              href={`/shipments/${shipment.id}`}
              onClick={() => toast.dismiss(t.id)}
              className="text-sm font-medium text-primary underline mt-1"
            >
              Track shipment →
            </a>
          </div>
        ),
        { duration: 8000 }
      );
    });
    return unsub;
  }, [onShipmentStatusChanged]);

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
