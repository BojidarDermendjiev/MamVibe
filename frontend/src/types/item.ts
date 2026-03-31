export const ListingType = {
  Donate: 0,
  Sell: 1,
} as const;
export type ListingType = (typeof ListingType)[keyof typeof ListingType];

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
  isActive: boolean;
  viewCount: number;
  likeCount: number;
  isLikedByCurrentUser: boolean;
  photos: ItemPhoto[];
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
