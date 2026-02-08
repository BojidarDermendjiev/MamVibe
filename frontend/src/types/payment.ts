export const PaymentMethod = {
  Card: 0,
  OnSpot: 1,
  Booking: 2,
} as const;
export type PaymentMethod = (typeof PaymentMethod)[keyof typeof PaymentMethod];

export const PaymentStatus = {
  Pending: 0,
  Completed: 1,
  Failed: 2,
  Cancelled: 3,
} as const;
export type PaymentStatus = (typeof PaymentStatus)[keyof typeof PaymentStatus];

export interface Payment {
  id: string;
  itemId: string;
  itemTitle: string;
  buyerId: string;
  sellerId: string;
  amount: number;
  paymentMethod: PaymentMethod;
  status: PaymentStatus;
  createdAt: string;
  receiptUrl: string | null;
}
