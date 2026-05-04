import axiosClient from './axiosClient';

export enum PlaceType {
  Walk = 0,
  Playground = 1,
  Restaurant = 2,
  Cafe = 3,
  Museum = 4,
  Zoo = 5,
  Beach = 6,
  Park = 7,
  ThemeAttraction = 8,
  SportsActivity = 9,
  Other = 10,
}

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
  website?: string;
}

export const childFriendlyPlacesApi = {
  getAll: () =>
    axiosClient.get<ChildFriendlyPlaceDto[]>('/child-friendly-places').then((r) => r.data),

  create: (dto: CreateChildFriendlyPlaceDto) =>
    axiosClient.post<ChildFriendlyPlaceDto>('/child-friendly-places', dto).then((r) => r.data),

  delete: (id: string) =>
    axiosClient.delete(`/child-friendly-places/${id}`),

  getPending: () =>
    axiosClient
      .get<ChildFriendlyPlaceDto[]>('/admin/child-friendly-places/pending')
      .then((r) => r.data),

  approve: (id: string) =>
    axiosClient.post(`/admin/child-friendly-places/${id}/approve`),

  adminDelete: (id: string) =>
    axiosClient.delete(`/admin/child-friendly-places/${id}`),
};

export const placeTypeLabel: Record<PlaceType, string> = {
  [PlaceType.Walk]: '🚶 Walk',
  [PlaceType.Playground]: '🛝 Playground',
  [PlaceType.Restaurant]: '🍽️ Restaurant',
  [PlaceType.Cafe]: '☕ Café',
  [PlaceType.Museum]: '🏛️ Museum',
  [PlaceType.Zoo]: '🦁 Zoo',
  [PlaceType.Beach]: '🏖️ Beach',
  [PlaceType.Park]: '🌳 Park',
  [PlaceType.ThemeAttraction]: '🎡 Theme Attraction',
  [PlaceType.SportsActivity]: '⚽ Sports',
  [PlaceType.Other]: '📍 Other',
};
