import { type ListingType } from './item';

export const PurchaseRequestStatus = {
  Pending: 0,
  Accepted: 1,
  Declined: 2,
  Cancelled: 3,
  Completed: 4,
} as const;
export type PurchaseRequestStatus = (typeof PurchaseRequestStatus)[keyof typeof PurchaseRequestStatus];

export interface PurchaseRequest {
  id: string;
  itemId: string | null;
  itemTitle: string | null;
  itemPhotoUrl: string | null;
  listingType: ListingType | null;
  price: number | null;
  buyerId: string;
  buyerDisplayName: string | null;
  buyerAvatarUrl: string | null;
  sellerId: string;
  status: PurchaseRequestStatus;
  createdAt: string;
  shipmentId?: string;
  bundleId?: string | null;
  bundleTitle?: string | null;
  bundlePhotoUrl?: string | null;
}
