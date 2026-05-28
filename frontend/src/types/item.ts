export const ListingType = {
  Donate: 0,
  Sell: 1,
} as const;
export type ListingType = (typeof ListingType)[keyof typeof ListingType];

export const ItemCondition = {
  Unspecified: 0,
  NewWithTags: 1,
  LikeNew: 2,
  Good: 3,
  Fair: 4,
} as const;
export type ItemCondition = (typeof ItemCondition)[keyof typeof ItemCondition];

export const AgeGroup = {
  Newborn: 0,
  Infant: 1,
  Toddler: 2,
  Preschool: 3,
  SchoolAge: 4,
  Teen: 5,
} as const;
export type AgeGroup = (typeof AgeGroup)[keyof typeof AgeGroup];

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
  ageGroup: AgeGroup | null;
  shoeSize: number | null;
  clothingSize: number | null;
  price: number | null;
  userId: string;
  userDisplayName: string;
  userAvatarUrl: string | null;
  userIsOnHoliday: boolean;
  condition: ItemCondition;
  isActive: boolean;
  isReserved: boolean;
  isSold: boolean;
  viewCount: number;
  likeCount: number;
  isLikedByCurrentUser: boolean;
  photos: ItemPhoto[];
  bumpedAt: string | null;
  createdAt: string;
  aiModerationStatus: number;
  aiModerationNotes: string | null;
  aiModerationScore: number | null;
}

export interface CreateItemRequest {
  title: string;
  description: string;
  categoryId: string;
  listingType: ListingType;
  ageGroup?: AgeGroup | null;
  shoeSize?: number | null;
  clothingSize?: number | null;
  price: number | null;
  condition?: ItemCondition;
  photoUrls?: string[];
}

export interface UpdateItemRequest {
  title: string;
  description: string;
  categoryId: string;
  listingType: ListingType;
  ageGroup?: AgeGroup | null;
  shoeSize?: number | null;
  clothingSize?: number | null;
  price: number | null;
  condition?: ItemCondition;
  photoUrls?: string[];
}

export interface ItemFilter {
  categoryId?: string;
  listingType?: ListingType;
  searchTerm?: string;
  brand?: string;
  ageGroup?: AgeGroup;
  shoeSize?: number;
  clothingSize?: number;
  condition?: ItemCondition;
  page: number;
  pageSize: number;
  sortBy: string;
}

export interface AiListingSuggestion {
  title: string;
  description: string;
  categorySlug: string;
  listingType: ListingType;
  suggestedPrice: number | null;
  ageGroup: AgeGroup | null;
  clothingSize: number | null;
  shoeSize: number | null;
}

export interface PriceSuggestionRequest {
  categoryId: string;
  title: string;
  description: string;
  ageGroup?: AgeGroup | null;
  clothingSize?: number | null;
  shoeSize?: number | null;
}

export interface PriceSuggestion {
  suggestedPrice: number | null;
  low: number | null;
  high: number | null;
  confidence: number;
  reason: string;
  comparableCount: number;
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

export interface PriceDropNotification {
  itemId: string;
  itemTitle: string;
  oldPrice: number;
  newPrice: number;
  photoUrl: string | null;
}
