export const PaymentMethod = {
  Card: 0,
  OnSpot: 1,
  Booking: 2,
  CashOnDelivery: 4,
} as const;
export type PaymentMethod = (typeof PaymentMethod)[keyof typeof PaymentMethod];

export const PaymentStatus = {
  Pending: 0,
  // Legacy non-escrow capture — kept for OnSpot / Booking / CashOnDelivery and for
  // pre-escrow Card payments.
  Completed: 1,
  Failed: 2,
  Cancelled: 3,
  // Escrow lifecycle (Phase B.2+). Funds captured to platform balance.
  HeldInEscrow: 4,
  // Funds released to the seller's Stripe Connect account.
  Released: 5,
  // Buyer fully refunded (product + courier) — used for lost-in-transit.
  RefundedFull: 6,
  // Buyer refunded for the product amount only — courier fee retained.
  RefundedProduct: 7,
  // Buyer raised a return / report — release paused pending admin resolution.
  Disputed: 8,
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
  // Escrow snapshot — zero / null on legacy non-escrow rows.
  sellerNetAmount: number;
  platformFeeAmount: number;
  heldUntil: string | null;
}
