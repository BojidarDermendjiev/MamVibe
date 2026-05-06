import { createContext, useContext, useEffect, useState, useCallback, useRef, type ReactNode } from 'react';
import toast from '@/utils/toast';
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
  const { onMessage, onPurchaseRequest, onSellerShipmentReady, onShipmentStatusChanged } = useSignalR();
  const [unreadCount, setUnreadCount] = useState(0);
  const [pendingRequestCount, setPendingRequestCount] = useState(0);
  const activeChatRef = useRef<string | null>(null);

  // Reset counts on logout; fetch initial counts on login
  useEffect(() => {
    if (!isAuthenticated) {
      setUnreadCount(0);
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
      .catch((err) => console.warn('[NotificationContext] Failed to load conversations:', err));

    purchaseRequestsApi
      .getAsSeller()
      .then((res) => {
        if (!cancelled) {
          const pending = res.data.filter((r) => r.status === PurchaseRequestStatus.Pending).length;
          setPendingRequestCount(pending);
        }
      })
      .catch((err) => console.warn('[NotificationContext] Failed to load purchase requests:', err));

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

  // Notify SELLER when waybill is generated — they need to print it and ship the package
  useEffect(() => {
    const unsub = onSellerShipmentReady((shipment) => {
      const courierNames: Record<number, string> = {
        [CourierProvider.Econt]: 'Econt',
        [CourierProvider.Speedy]: 'Speedy',
        [CourierProvider.BoxNow]: 'Box Now',
      };
      const courier = courierNames[shipment.courierProvider] ?? '';
      toast(
        (t) => (
          <div className="flex flex-col gap-1 bg-white dark:bg-[#2d2a42] rounded-xl px-4 py-3.5 shadow-xl border border-gray-100 dark:border-white/10 border-l-4 border-l-[#945c67] min-w-[280px] max-w-[380px]">
            <p className="font-semibold text-primary">🖨️ Waybill ready to print!</p>
            <p className="text-sm text-gray-600">
              {shipment.itemTitle && <span>"{shipment.itemTitle}" via {courier}</span>}
              {shipment.trackingNumber && <span> · #{shipment.trackingNumber}</span>}
            </p>
            <p className="text-xs text-gray-500">Print the label, attach it to the package and hand it to the courier.</p>
            <a
              href={`/shipments/${shipment.id}`}
              onClick={() => toast.dismiss(t.id)}
              className="text-sm font-medium text-primary underline mt-1"
            >
              View waybill & download label →
            </a>
          </div>
        ),
        { duration: 12000 }
      );
    });
    return unsub;
  }, [onSellerShipmentReady]);

  // Notify BUYER when courier picks up the package — their order is on its way
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
          <div className="flex flex-col gap-1 bg-white dark:bg-[#2d2a42] rounded-xl px-4 py-3.5 shadow-xl border border-gray-100 dark:border-white/10 border-l-4 border-l-emerald-400 min-w-[280px] max-w-[380px]">
            <p className="font-semibold text-primary">🚚 Your order is on its way!</p>
            <p className="text-sm text-gray-600">
              {shipment.itemTitle && <span>"{shipment.itemTitle}" </span>}
              is being shipped via {courier}.
              {shipment.trackingNumber && <span> Track: #{shipment.trackingNumber}</span>}
            </p>
            <a
              href={`/shipments/${shipment.id}`}
              onClick={() => toast.dismiss(t.id)}
              className="text-sm font-medium text-primary underline mt-1"
            >
              Track your package →
            </a>
          </div>
        ),
        { duration: 10000 }
      );
    });
    return unsub;
  }, [onShipmentStatusChanged]);

  const setActiveChatUserId = useCallback((userId: string | null) => {
    activeChatRef.current = userId;
  }, []);

  const markConversationRead = useCallback((userId: string) => {
    messagesApi.markAsRead(userId).catch((err) => console.warn('[NotificationContext] markAsRead failed:', err));
    // Re-fetch to get accurate count
    if (isAuthenticated) {
      messagesApi
        .getConversations()
        .then((res) => {
          const total = res.data.reduce((sum, c) => sum + c.unreadCount, 0);
          setUnreadCount(total);
        })
        .catch((err) => console.warn('[NotificationContext] Failed to refresh unread count:', err));
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
