export const CourierProvider = {
  Econt: 0,
  Speedy: 1,
  BoxNow: 2,
} as const;
export type CourierProvider = (typeof CourierProvider)[keyof typeof CourierProvider];

export const DeliveryType = {
  Office: 0,
  Address: 1,
  Locker: 2,
} as const;
export type DeliveryType = (typeof DeliveryType)[keyof typeof DeliveryType];

export const ShipmentStatus = {
  Pending: 0,
  Created: 1,
  PickedUp: 2,
  InTransit: 3,
  OutForDelivery: 4,
  Delivered: 5,
  Returned: 6,
  Cancelled: 7,
} as const;
export type ShipmentStatus = (typeof ShipmentStatus)[keyof typeof ShipmentStatus];

export interface Shipment {
  id: string;
  paymentId: string;
  itemTitle: string | null;
  courierProvider: CourierProvider;
  deliveryType: DeliveryType;
  status: ShipmentStatus;
  trackingNumber: string | null;
  waybillId: string | null;
  recipientName: string;
  recipientPhone: string;
  deliveryAddress: string | null;
  city: string | null;
  officeId: string | null;
  officeName: string | null;
  shippingPrice: number;
  isCod: boolean;
  codAmount: number;
  isInsured: boolean;
  insuredAmount: number;
  weight: number;
  labelUrl: string | null;
  createdAt: string;
}

export interface CourierOffice {
  id: string;
  name: string;
  city: string | null;
  address: string | null;
  lat: number | null;
  lng: number | null;
  isLocker: boolean;
}

export interface ShippingPriceResult {
  price: number;
  currency: string;
  estimatedDelivery: string | null;
}

export interface TrackingEvent {
  timestamp: string;
  description: string;
  location: string | null;
}

export interface CalculateShippingRequest {
  courierProvider: CourierProvider;
  deliveryType: DeliveryType;
  fromCity?: string;
  toCity?: string;
  officeId?: string;
  weight: number;
  isCod: boolean;
  codAmount: number;
  isInsured: boolean;
  insuredAmount: number;
}

export interface CreateShipmentRequest {
  paymentId: string;
  courierProvider: CourierProvider;
  deliveryType: DeliveryType;
  recipientName: string;
  recipientPhone: string;
  deliveryAddress?: string;
  city?: string;
  officeId?: string;
  officeName?: string;
  weight: number;
  isCod: boolean;
  codAmount: number;
  isInsured: boolean;
  insuredAmount: number;
}
