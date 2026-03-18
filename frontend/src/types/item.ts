export const ListingType = {
  Donate: 0,
  Sell: 1,
} as const;
export type ListingType = (typeof ListingType)[keyof typeof ListingType];

export interface ItemPhoto {
  id: string;
  url: string;
  displayOrder: number;
}

export interface Item {
  id: string;
  title: string;
  description: string;
  categoryId: string;
  categoryName: string;
  listingType: ListingType;
  price: number | null;
  userId: string;
  userDisplayName: string;
  userAvatarUrl: string | null;
  isActive: boolean;
  viewCount: number;
  likeCount: number;
  isLikedByCurrentUser: boolean;
  photos: ItemPhoto[];
  createdAt: string;
}

export interface CreateItemRequest {
  title: string;
  description: string;
  categoryId: string;
  listingType: ListingType;
  price: number | null;
  photoUrls?: string[];
}

export interface UpdateItemRequest {
  title: string;
  description: string;
  categoryId: string;
  listingType: ListingType;
  price: number | null;
  photoUrls?: string[];
}

export interface ItemFilter {
  categoryId?: string;
  listingType?: ListingType;
  searchTerm?: string;
  brand?: string;
  page: number;
  pageSize: number;
  sortBy: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Category {
  id: string;
  name: string;
  description: string;
  slug: string;
}
