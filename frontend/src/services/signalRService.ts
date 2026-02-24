import * as signalR from '@microsoft/signalr';
import type { Message } from '../types/message';
import type { PurchaseRequest } from '../types/purchaseRequest';
import type { Shipment } from '../types/shipping';

type MessageHandler = (message: Message) => void;
type ReadHandler = (senderId: string) => void;
type TypingHandler = (userId: string) => void;
type OnlineHandler = (userId: string) => void;
type PurchaseRequestHandler = (request: PurchaseRequest) => void;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type PaymentChosenHandler = (notification: any) => void;
type ShipmentHandler = (shipment: Shipment) => void;

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
  private shipmentCreatedHandlers: ShipmentHandler[] = [];
  private shipmentStatusChangedHandlers: ShipmentHandler[] = [];

  async connect(accessToken: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('ReceiveMessage', (message: Message) => {
      this.messageHandlers.forEach((h) => h(message));
    });

    this.connection.on('MessageRead', (senderId: string) => {
      this.readHandlers.forEach((h) => h(senderId));
    });

    this.connection.on('UserTyping', (userId: string) => {
      this.typingHandlers.forEach((h) => h(userId));
    });

    this.connection.on('UserOnline', (userId: string) => {
      this.onlineHandlers.forEach((h) => h(userId));
    });

    this.connection.on('UserOffline', (userId: string) => {
      this.offlineHandlers.forEach((h) => h(userId));
    });

    this.connection.on('ReceivePurchaseRequest', (request: PurchaseRequest) => {
      this.purchaseRequestHandlers.forEach((h) => h(request));
    });

    this.connection.on('PurchaseRequestUpdated', (request: PurchaseRequest) => {
      this.purchaseRequestUpdatedHandlers.forEach((h) => h(request));
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    this.connection.on('PaymentMethodChosen', (notification: any) => {
      this.paymentChosenHandlers.forEach((h) => h(notification));
    });

    this.connection.on('ShipmentCreated', (shipment: Shipment) => {
      this.shipmentCreatedHandlers.forEach((h) => h(shipment));
    });

    this.connection.on('ShipmentStatusChanged', (shipment: Shipment) => {
      this.shipmentStatusChangedHandlers.forEach((h) => h(shipment));
    });

    await this.connection.start();
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  async sendMessage(receiverId: string, content: string): Promise<Message | null> {
    const message = await this.connection?.invoke<Message>('SendMessage', receiverId, content);
    return message ?? null;
  }

  async markAsRead(senderId: string): Promise<void> {
    await this.connection?.invoke('MarkAsRead', senderId);
  }

  async sendTyping(receiverId: string): Promise<void> {
    await this.connection?.invoke('SendTyping', receiverId);
  }

  onMessage(handler: MessageHandler): () => void {
    this.messageHandlers.push(handler);
    return () => {
      this.messageHandlers = this.messageHandlers.filter((h) => h !== handler);
    };
  }

  onRead(handler: ReadHandler): () => void {
    this.readHandlers.push(handler);
    return () => {
      this.readHandlers = this.readHandlers.filter((h) => h !== handler);
    };
  }

  onTyping(handler: TypingHandler): () => void {
    this.typingHandlers.push(handler);
    return () => {
      this.typingHandlers = this.typingHandlers.filter((h) => h !== handler);
    };
  }

  onOnline(handler: OnlineHandler): () => void {
    this.onlineHandlers.push(handler);
    return () => {
      this.onlineHandlers = this.onlineHandlers.filter((h) => h !== handler);
    };
  }

  onOffline(handler: OnlineHandler): () => void {
    this.offlineHandlers.push(handler);
    return () => {
      this.offlineHandlers = this.offlineHandlers.filter((h) => h !== handler);
    };
  }

  onPurchaseRequest(handler: PurchaseRequestHandler): () => void {
    this.purchaseRequestHandlers.push(handler);
    return () => {
      this.purchaseRequestHandlers = this.purchaseRequestHandlers.filter((h) => h !== handler);
    };
  }

  onPurchaseRequestUpdated(handler: PurchaseRequestHandler): () => void {
    this.purchaseRequestUpdatedHandlers.push(handler);
    return () => {
      this.purchaseRequestUpdatedHandlers = this.purchaseRequestUpdatedHandlers.filter((h) => h !== handler);
    };
  }

  onPaymentChosen(handler: PaymentChosenHandler): () => void {
    this.paymentChosenHandlers.push(handler);
    return () => {
      this.paymentChosenHandlers = this.paymentChosenHandlers.filter((h) => h !== handler);
    };
  }

  onShipmentCreated(handler: ShipmentHandler): () => void {
    this.shipmentCreatedHandlers.push(handler);
    return () => {
      this.shipmentCreatedHandlers = this.shipmentCreatedHandlers.filter((h) => h !== handler);
    };
  }

  onShipmentStatusChanged(handler: ShipmentHandler): () => void {
    this.shipmentStatusChangedHandlers.push(handler);
    return () => {
      this.shipmentStatusChangedHandlers = this.shipmentStatusChangedHandlers.filter((h) => h !== handler);
    };
  }
}

export const signalRService = new SignalRService();
