import { createContext, useContext, useEffect, useRef, useState, useCallback, type ReactNode } from 'react';
import { signalRService } from '../services/signalRService';
import { useAuthStore } from '../store/authStore';
import type { Message } from '../types/message';
import type { PurchaseRequest } from '../types/purchaseRequest';
import type { Shipment } from '../types/shipping';

type MessageHandler = (message: Message) => void;
type TypingHandler = (userId: string) => void;
type ReadHandler = (senderId: string) => void;
type OnlineHandler = (userId: string) => void;
type PurchaseRequestHandler = (request: PurchaseRequest) => void;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type PaymentChosenHandler = (notification: any) => void;
type ShipmentHandler = (shipment: Shipment) => void;

interface SignalRContextValue {
  isConnected: boolean;
  sendMessage: (receiverId: string, content: string) => Promise<Message | null>;
  sendTyping: (receiverId: string) => Promise<void>;
  markAsRead: (senderId: string) => Promise<void>;
  onMessage: (handler: MessageHandler) => () => void;
  onTyping: (handler: TypingHandler) => () => void;
  onRead: (handler: ReadHandler) => () => void;
  onOnline: (handler: OnlineHandler) => () => void;
  onOffline: (handler: OnlineHandler) => () => void;
  onPurchaseRequest: (handler: PurchaseRequestHandler) => () => void;
  onPurchaseRequestUpdated: (handler: PurchaseRequestHandler) => () => void;
  onPaymentChosen: (handler: PaymentChosenHandler) => () => void;
  onSellerShipmentReady: (handler: ShipmentHandler) => () => void;
  onShipmentStatusChanged: (handler: ShipmentHandler) => () => void;
}

const SignalRContext = createContext<SignalRContextValue>({
  isConnected: false,
  sendMessage: async () => null,
  sendTyping: async () => {},
  markAsRead: async () => {},
  onMessage: () => () => {},
  onTyping: () => () => {},
  onRead: () => () => {},
  onOnline: () => () => {},
  onOffline: () => () => {},
  onPurchaseRequest: () => () => {},
  onPurchaseRequestUpdated: () => () => {},
  onPaymentChosen: () => () => {},
  onSellerShipmentReady: () => () => {},
  onShipmentStatusChanged: () => () => {},
});

export function SignalRProvider({ children }: { children: ReactNode }) {
  const { accessToken, isAuthenticated } = useAuthStore();
  const [isConnected, setIsConnected] = useState(false);
  const connectingRef = useRef(false);

  useEffect(() => {
    if (isAuthenticated && accessToken && !connectingRef.current) {
      connectingRef.current = true;
      signalRService
        .connect(accessToken)
        .then(() => setIsConnected(true))
        .catch(() => {})
        .finally(() => { connectingRef.current = false; });
    }

    if (!isAuthenticated) {
      signalRService.disconnect().then(() => setIsConnected(false)).catch(() => {});
    }
  }, [isAuthenticated, accessToken]);

  const sendMessage = useCallback(
    (receiverId: string, content: string) => signalRService.sendMessage(receiverId, content),
    []
  );

  const sendTyping = useCallback(
    (receiverId: string) => signalRService.sendTyping(receiverId),
    []
  );

  const markAsRead = useCallback(
    (senderId: string) => signalRService.markAsRead(senderId),
    []
  );

  const onMessage = useCallback(
    (handler: MessageHandler) => signalRService.onMessage(handler),
    []
  );

  const onTyping = useCallback(
    (handler: TypingHandler) => signalRService.onTyping(handler),
    []
  );

  const onRead = useCallback(
    (handler: ReadHandler) => signalRService.onRead(handler),
    []
  );

  const onOnline = useCallback(
    (handler: OnlineHandler) => signalRService.onOnline(handler),
    []
  );

  const onOffline = useCallback(
    (handler: OnlineHandler) => signalRService.onOffline(handler),
    []
  );

  const onPurchaseRequest = useCallback(
    (handler: PurchaseRequestHandler) => signalRService.onPurchaseRequest(handler),
    []
  );

  const onPurchaseRequestUpdated = useCallback(
    (handler: PurchaseRequestHandler) => signalRService.onPurchaseRequestUpdated(handler),
    []
  );

  const onPaymentChosen = useCallback(
    (handler: PaymentChosenHandler) => signalRService.onPaymentChosen(handler),
    []
  );

  const onSellerShipmentReady = useCallback(
    (handler: ShipmentHandler) => signalRService.onSellerShipmentReady(handler),
    []
  );

  const onShipmentStatusChanged = useCallback(
    (handler: ShipmentHandler) => signalRService.onShipmentStatusChanged(handler),
    []
  );

  return (
    <SignalRContext.Provider
      value={{
        isConnected,
        sendMessage,
        sendTyping,
        markAsRead,
        onMessage,
        onTyping,
        onRead,
        onOnline,
        onOffline,
        onPurchaseRequest,
        onPurchaseRequestUpdated,
        onPaymentChosen,
        onSellerShipmentReady,
        onShipmentStatusChanged,
      }}
    >
      {children}
    </SignalRContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useSignalR(): SignalRContextValue {
  return useContext(SignalRContext);
}
