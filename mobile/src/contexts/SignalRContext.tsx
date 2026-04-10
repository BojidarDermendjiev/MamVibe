import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import { signalRService } from '@/services/signalRService';
import { useAuthStore } from '@/store/authStore';
import type { Message, PurchaseRequest, Shipment } from '@mamvibe/shared';

type MessageHandler          = (msg: Message) => void;
type TypingHandler           = (userId: string) => void;
type ReadHandler             = (senderId: string) => void;
type OnlineHandler           = (userId: string) => void;
type PurchaseRequestHandler  = (req: PurchaseRequest) => void;
type PaymentChosenHandler    = (n: any) => void;
type ShipmentHandler         = (s: Shipment) => void;

interface SignalRContextValue {
  isConnected: boolean;
  sendMessage:             (receiverId: string, content: string) => Promise<Message | null>;
  sendTyping:              (receiverId: string) => Promise<void>;
  markAsRead:              (senderId: string) => Promise<void>;
  onMessage:               (h: MessageHandler)         => () => void;
  onTyping:                (h: TypingHandler)           => () => void;
  onRead:                  (h: ReadHandler)             => () => void;
  onOnline:                (h: OnlineHandler)           => () => void;
  onOffline:               (h: OnlineHandler)           => () => void;
  onPurchaseRequest:       (h: PurchaseRequestHandler)  => () => void;
  onPurchaseRequestUpdated:(h: PurchaseRequestHandler)  => () => void;
  onPaymentChosen:         (h: PaymentChosenHandler)    => () => void;
  onSellerShipmentReady:   (h: ShipmentHandler)         => () => void;
  onShipmentStatusChanged: (h: ShipmentHandler)         => () => void;
}

const noop = () => () => {};

const SignalRContext = createContext<SignalRContextValue>({
  isConnected: false,
  sendMessage: async () => null,
  sendTyping: async () => {},
  markAsRead: async () => {},
  onMessage: noop, onTyping: noop, onRead: noop, onOnline: noop, onOffline: noop,
  onPurchaseRequest: noop, onPurchaseRequestUpdated: noop,
  onPaymentChosen: noop, onSellerShipmentReady: noop, onShipmentStatusChanged: noop,
});

export function SignalRProvider({ children }: { children: ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [isConnected, setIsConnected] = useState(false);
  const connectingRef = useRef(false);

  useEffect(() => {
    if (isAuthenticated && !connectingRef.current) {
      connectingRef.current = true;
      signalRService
        .connect()
        .then(() => setIsConnected(true))
        .catch(() => {})
        .finally(() => { connectingRef.current = false; });
    }
    if (!isAuthenticated) {
      signalRService.disconnect().then(() => setIsConnected(false)).catch(() => {});
    }
  }, [isAuthenticated]);

  const sendMessage    = useCallback((r: string, c: string) => signalRService.sendMessage(r, c), []);
  const sendTyping     = useCallback((r: string) => signalRService.sendTyping(r), []);
  const markAsRead     = useCallback((s: string) => signalRService.markAsRead(s), []);
  const onMessage      = useCallback((h: MessageHandler)         => signalRService.onMessage(h), []);
  const onTyping       = useCallback((h: TypingHandler)           => signalRService.onTyping(h), []);
  const onRead         = useCallback((h: ReadHandler)             => signalRService.onRead(h), []);
  const onOnline       = useCallback((h: OnlineHandler)           => signalRService.onOnline(h), []);
  const onOffline      = useCallback((h: OnlineHandler)           => signalRService.onOffline(h), []);
  const onPurchaseRequest        = useCallback((h: PurchaseRequestHandler)  => signalRService.onPurchaseRequest(h), []);
  const onPurchaseRequestUpdated = useCallback((h: PurchaseRequestHandler)  => signalRService.onPurchaseRequestUpdated(h), []);
  const onPaymentChosen          = useCallback((h: PaymentChosenHandler)    => signalRService.onPaymentChosen(h), []);
  const onSellerShipmentReady    = useCallback((h: ShipmentHandler)         => signalRService.onSellerShipmentReady(h), []);
  const onShipmentStatusChanged  = useCallback((h: ShipmentHandler)         => signalRService.onShipmentStatusChanged(h), []);

  return (
    <SignalRContext.Provider value={{
      isConnected, sendMessage, sendTyping, markAsRead,
      onMessage, onTyping, onRead, onOnline, onOffline,
      onPurchaseRequest, onPurchaseRequestUpdated,
      onPaymentChosen, onSellerShipmentReady, onShipmentStatusChanged,
    }}>
      {children}
    </SignalRContext.Provider>
  );
}

export function useSignalR() {
  return useContext(SignalRContext);
}
