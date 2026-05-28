import type { ListingType, AgeGroup, ItemCondition, Item } from './item';

export interface SavedSearchDto {
  id: string;
  name: string;
  categoryId: string | null;
  categoryName: string | null;
  listingType: ListingType | null;
  searchTerm: string | null;
  ageGroup: AgeGroup | null;
  shoeSize: number | null;
  clothingSize: number | null;
  condition: ItemCondition | null;
  maxPrice: number | null;
  createdAt: string;
}

export interface CreateSavedSearchDto {
  name: string;
  categoryId?: string | null;
  listingType?: ListingType | null;
  searchTerm?: string | null;
  ageGroup?: AgeGroup | null;
  shoeSize?: number | null;
  clothingSize?: number | null;
  condition?: number | null;
  maxPrice?: number | null;
}

export interface SavedSearchMatchNotification {
  savedSearchId: string;
  savedSearchName: string;
  item: Item;
}
