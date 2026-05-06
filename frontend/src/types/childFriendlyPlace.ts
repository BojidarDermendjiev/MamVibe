export const PlaceType = {
  Walk: 0,
  Playground: 1,
  Restaurant: 2,
  Cafe: 3,
  Museum: 4,
  Zoo: 5,
  Beach: 6,
  Park: 7,
  ThemeAttraction: 8,
  SportsActivity: 9,
  Other: 10,
} as const;

export type PlaceType = (typeof PlaceType)[keyof typeof PlaceType];

export interface ChildFriendlyPlaceDto {
  id: string;
  userId: string;
  authorDisplayName: string | null;
  name: string;
  description: string;
  address: string | null;
  city: string;
  placeType: PlaceType;
  ageFromMonths: number | null;
  ageToMonths: number | null;
  photoUrl: string | null;
  website: string | null;
  isApproved: boolean;
  createdAt: string;
}

export interface CreateChildFriendlyPlaceDto {
  name: string;
  description: string;
  address?: string;
  city: string;
  placeType: PlaceType;
  ageFromMonths?: number;
  ageToMonths?: number;
  photoUrl?: string;
  website?: string;
}
