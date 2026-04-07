import type { PaymentMethod } from './payment';

export interface EBill {
  id: string;
  eBillNumber: string | null;
  itemId: string;
  itemTitle: string | null;
  buyerId: string;
  sellerId: string;
  sellerDisplayName: string | null;
  amount: number;
  currency: string;
  paymentMethod: PaymentMethod;
  issuedAt: string;
  receiptUrl: string | null;
}
