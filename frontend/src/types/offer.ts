export const OfferStatus = {
  Pending: 0,
  Accepted: 1,
  Declined: 2,
  Countered: 3,
  Expired: 4,
  Cancelled: 5,
} as const;
export type OfferStatus = (typeof OfferStatus)[keyof typeof OfferStatus];

export interface Offer {
  id: string;
  itemId: string;
  itemTitle: string | null;
  itemPhotoUrl: string | null;
  itemPrice: number | null;
  buyerDisplayName: string | null;
  buyerAvatarUrl: string | null;
  sellerDisplayName: string | null;
  offeredPrice: number;
  counterPrice: number | null;
  status: OfferStatus;
  createdAt: string;
  updatedAt: string | null;
}
