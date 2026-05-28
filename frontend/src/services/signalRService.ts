import * as signalR from '@microsoft/signalr';
import type { Message } from '../types/message';
import type { PurchaseRequest } from '../types/purchaseRequest';
import type { Shipment } from '../types/shipping';
import type { NewFollowerNotification } from '../types/follow';
import type { Item } from '../types/item';
import type { SavedSearchMatchNotification } from '../types/savedSearch';

type MessageHandler = (message: Message) => void;
type ReadHandler = (senderId: string) => void;
type TypingHandler = (userId: string) => void;
type OnlineHandler = (userId: string) => void;
type PurchaseRequestHandler = (request: PurchaseRequest) => void;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type PaymentChosenHandler = (notification: any) => void;
type ShipmentHandler = (shipment: Shipment) => void;
type NewFollowerHandler = (notification: NewFollowerNotification) => void;
type NewItemFromFollowedSellerHandler = (item: Item) => void;
type SavedSearchMatchHandler = (notification: SavedSearchMatchNotification) => void;

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
  private newFollowerHandlers: NewFollowerHandler[] = [];
  private newItemFromFollowedSellerHandlers: NewItemFromFollowedSellerHandler[] = [];
  private savedSearchMatchHandlers: SavedSearchMatchHandler[] = [];

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
      this.sellerShipmentReadyHandlers.forEach((h) => h(shipment));
    });

    this.connection.on('ShipmentStatusChanged', (shipment: Shipment) => {
      this.shipmentStatusChangedHandlers.forEach((h) => h(shipment));
    });

    this.connection.on('NewFollower', (notification: NewFollowerNotification) => {
      this.newFollowerHandlers.forEach((h) => h(notification));
    });

    this.connection.on('NewItemFromFollowedSeller', (item: Item) => {
      this.newItemFromFollowedSellerHandlers.forEach((h) => h(item));
    });

    this.connection.on('SavedSearchMatch', (notification: SavedSearchMatchNotification) => {
      this.savedSearchMatchHandlers.forEach((h) => h(notification));
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

  onSellerShipmentReady(handler: ShipmentHandler): () => void {
    this.sellerShipmentReadyHandlers.push(handler);
    return () => {
      this.sellerShipmentReadyHandlers = this.sellerShipmentReadyHandlers.filter((h) => h !== handler);
    };
  }

  onShipmentStatusChanged(handler: ShipmentHandler): () => void {
    this.shipmentStatusChangedHandlers.push(handler);
    return () => {
      this.shipmentStatusChangedHandlers = this.shipmentStatusChangedHandlers.filter((h) => h !== handler);
    };
  }

  onNewFollower(handler: NewFollowerHandler): () => void {
    this.newFollowerHandlers.push(handler);
    return () => {
      this.newFollowerHandlers = this.newFollowerHandlers.filter((h) => h !== handler);
    };
  }

  onNewItemFromFollowedSeller(handler: NewItemFromFollowedSellerHandler): () => void {
    this.newItemFromFollowedSellerHandlers.push(handler);
    return () => {
      this.newItemFromFollowedSellerHandlers = this.newItemFromFollowedSellerHandlers.filter((h) => h !== handler);
    };
  }

  onSavedSearchMatch(handler: SavedSearchMatchHandler): () => void {
    this.savedSearchMatchHandlers.push(handler);
    return () => {
      this.savedSearchMatchHandlers = this.savedSearchMatchHandlers.filter((h) => h !== handler);
    };
  }
}

export const signalRService = new SignalRService();
