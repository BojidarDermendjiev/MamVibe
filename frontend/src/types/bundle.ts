export interface BundleItemDto {
  itemId: string;
  title: string;
  price: number | null;
  photoUrl: string | null;
}

export interface BundleDto {
  id: string;
  title: string;
  description: string | null;
  price: number;
  sellerId: string;
  sellerDisplayName: string | null;
  sellerAvatarUrl: string | null;
  isActive: boolean;
  isSold: boolean;
  isOwnedByCurrentUser: boolean;
  items: BundleItemDto[];
  createdAt: string;
}

export interface CreateBundleDto {
  title: string;
  description: string | null;
  price: number;
  itemIds: string[];
}
