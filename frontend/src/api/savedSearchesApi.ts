import axiosClient from './axiosClient';
import type { SavedSearchDto, CreateSavedSearchDto } from '../types/savedSearch';

export const savedSearchesApi = {
  getMy: () =>
    axiosClient.get<SavedSearchDto[]>('/saved-searches'),

  create: (dto: CreateSavedSearchDto) =>
    axiosClient.post<SavedSearchDto>('/saved-searches', dto),

  delete: (id: string) =>
    axiosClient.delete(`/saved-searches/${id}`),
};
