import axiosClient from './axiosClient';
import type { ChildFriendlyPlaceDto, CreateChildFriendlyPlaceDto } from '../types/childFriendlyPlace';
import { PlaceType } from '../types/childFriendlyPlace';

export const childFriendlyPlacesApi = {
  getAll: (params?: { city?: string; placeType?: PlaceType; childAgeMonths?: number; page?: number; pageSize?: number }) =>
    axiosClient.get<ChildFriendlyPlaceDto[]>('/child-friendly-places', { params }).then(r => r.data),

  getById: (id: string) =>
    axiosClient.get<ChildFriendlyPlaceDto>(`/child-friendly-places/${id}`).then(r => r.data),

  create: (dto: CreateChildFriendlyPlaceDto) =>
    axiosClient.post<ChildFriendlyPlaceDto>('/child-friendly-places', dto).then(r => r.data),

  delete: (id: string) =>
    axiosClient.delete(`/child-friendly-places/${id}`),

  getPending: () =>
    axiosClient.get<ChildFriendlyPlaceDto[]>('/admin/child-friendly-places/pending').then(r => r.data),

  approve: (id: string) =>
    axiosClient.post(`/admin/child-friendly-places/${id}/approve`),

  adminDelete: (id: string) =>
    axiosClient.delete(`/admin/child-friendly-places/${id}`),
};
