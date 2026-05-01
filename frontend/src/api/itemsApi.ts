import axiosClient from './axiosClient';
import type { Item, CreateItemRequest, UpdateItemRequest, ItemFilter, PagedResult, Category } from '../types/item';
import type { SellerCheckResult } from './purchaseRequestsApi';

export const itemsApi = {
  getAll: (filter: ItemFilter) =>
    axiosClient.get<PagedResult<Item>>('/items', { params: filter }),

  getById: (id: string) =>
    axiosClient.get<Item>(`/items/${id}`),

  create: (data: CreateItemRequest) =>
    axiosClient.post<Item>('/items', data),

  update: (id: string, data: UpdateItemRequest) =>
    axiosClient.put<Item>(`/items/${id}`, data),

  delete: (id: string) =>
    axiosClient.delete(`/items/${id}`),

  toggleLike: (id: string) =>
    axiosClient.post(`/items/${id}/like`),

  incrementView: (id: string) =>
    axiosClient.post(`/items/${id}/view`),

  getCategories: () =>
    axiosClient.get<Category[]>('/categories'),

  getMyItems: () =>
    axiosClient.get<Item[]>('/users/dashboard/items'),

  getLikedItems: () =>
    axiosClient.get<Item[]>('/users/dashboard/liked'),

  checkSeller: (id: string) =>
    axiosClient.get<SellerCheckResult>(`/items/${id}/seller-check`),
};
