import * as signalR from '@microsoft/signalr';
import { ensureFreshToken } from '@/api/axiosClient';
import type { Message, PurchaseRequest, Shipment } from '@mamvibe/shared';

type MessageHandler = (message: Message) => void;
type ReadHandler = (senderId: string) => void;
type TypingHandler = (userId: string) => void;
type OnlineHandler = (userId: string) => void;
type PurchaseRequestHandler = (request: PurchaseRequest) => void;
type PaymentChosenHandler = (notification: any) => void;
type ShipmentHandler = (shipment: Shipment) => void;

const API_URL = process.env.EXPO_PUBLIC_API_URL ?? 'http://10.0.2.2:5038/api';
// Derive hub URL from API URL: strip trailing /api and append /hubs/chat
const HUB_URL = API_URL.replace(/\/api\/?$/, '') + '/hubs/chat';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private messageHandlers: MessageHandler[] = [];
  private readHandlers: ReadHandler[] = [];
  private typingHandlers: TypingHandler[] = [];
  private onlineHandlers: OnlineHandler[] = [];
  private offlineHandlers: OnlineHandler[] = [];
  private purchaseRequestHandlers: PurchaseRequestHandler[] = [];
  private purchaseRequestUpdatedHandlers: PurchaseRequestHandler[] = [];
  private paymentChosenHandlers: PaymentChosenHandler[] = [];
  private sellerShipmentReadyHandlers: ShipmentHandler[] = [];
  private shipmentStatusChangedHandlers: ShipmentHandler[] = [];

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        // Async factory — reads JWT from SecureStore each time SignalR needs it
        accessTokenFactory: ensureFreshToken,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('ReceiveMessage', (msg: Message) =>
      this.messageHandlers.forEach((h) => h(msg)),
    );
    this.connection.on('MessageRead', (senderId: string) =>
      this.readHandlers.forEach((h) => h(senderId)),
    );
    this.connection.on('UserTyping', (userId: string) =>
      this.typingHandlers.forEach((h) => h(userId)),
    );
    this.connection.on('UserOnline', (userId: string) =>
      this.onlineHandlers.forEach((h) => h(userId)),
    );
    this.connection.on('UserOffline', (userId: string) =>
      this.offlineHandlers.forEach((h) => h(userId)),
    );
    this.connection.on('ReceivePurchaseRequest', (req: PurchaseRequest) =>
      this.purchaseRequestHandlers.forEach((h) => h(req)),
    );
    this.connection.on('PurchaseRequestUpdated', (req: PurchaseRequest) =>
      this.purchaseRequestUpdatedHandlers.forEach((h) => h(req)),
    );
    this.connection.on('PaymentMethodChosen', (n: any) =>
      this.paymentChosenHandlers.forEach((h) => h(n)),
    );
    this.connection.on('ShipmentCreated', (s: Shipment) =>
      this.sellerShipmentReadyHandlers.forEach((h) => h(s)),
    );
    this.connection.on('ShipmentStatusChanged', (s: Shipment) =>
      this.shipmentStatusChangedHandlers.forEach((h) => h(s)),
    );

    await this.connection.start();
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  get isConnected() {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  async sendMessage(receiverId: string, content: string): Promise<Message | null> {
    return (await this.connection?.invoke<Message>('SendMessage', receiverId, content)) ?? null;
  }

  async markAsRead(senderId: string): Promise<void> {
    await this.connection?.invoke('MarkAsRead', senderId);
  }

  async sendTyping(receiverId: string): Promise<void> {
    await this.connection?.invoke('SendTyping', receiverId);
  }

  onMessage(h: MessageHandler)   { this.messageHandlers.push(h);   return () => { this.messageHandlers   = this.messageHandlers.filter((x) => x !== h); }; }
  onRead(h: ReadHandler)         { this.readHandlers.push(h);       return () => { this.readHandlers       = this.readHandlers.filter((x) => x !== h); }; }
  onTyping(h: TypingHandler)     { this.typingHandlers.push(h);     return () => { this.typingHandlers     = this.typingHandlers.filter((x) => x !== h); }; }
  onOnline(h: OnlineHandler)     { this.onlineHandlers.push(h);     return () => { this.onlineHandlers     = this.onlineHandlers.filter((x) => x !== h); }; }
  onOffline(h: OnlineHandler)    { this.offlineHandlers.push(h);    return () => { this.offlineHandlers    = this.offlineHandlers.filter((x) => x !== h); }; }
  onPurchaseRequest(h: PurchaseRequestHandler)        { this.purchaseRequestHandlers.push(h);        return () => { this.purchaseRequestHandlers        = this.purchaseRequestHandlers.filter((x) => x !== h); }; }
  onPurchaseRequestUpdated(h: PurchaseRequestHandler) { this.purchaseRequestUpdatedHandlers.push(h); return () => { this.purchaseRequestUpdatedHandlers = this.purchaseRequestUpdatedHandlers.filter((x) => x !== h); }; }
  onPaymentChosen(h: PaymentChosenHandler)            { this.paymentChosenHandlers.push(h);           return () => { this.paymentChosenHandlers           = this.paymentChosenHandlers.filter((x) => x !== h); }; }
  onSellerShipmentReady(h: ShipmentHandler)           { this.sellerShipmentReadyHandlers.push(h);    return () => { this.sellerShipmentReadyHandlers    = this.sellerShipmentReadyHandlers.filter((x) => x !== h); }; }
  onShipmentStatusChanged(h: ShipmentHandler)         { this.shipmentStatusChangedHandlers.push(h);  return () => { this.shipmentStatusChangedHandlers  = this.shipmentStatusChangedHandlers.filter((x) => x !== h); }; }
}

export const signalRService = new SignalRService();
