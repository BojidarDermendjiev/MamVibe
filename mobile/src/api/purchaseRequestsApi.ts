import axiosClient from './axiosClient';
import type { PurchaseRequest } from '@mamvibe/shared';

export const purchaseRequestsApi = {
  create: (itemId: string) =>
    axiosClient.post<PurchaseRequest>('/purchase-requests', { itemId }),

  accept: (id: string) =>
    axiosClient.post<PurchaseRequest>(`/purchase-requests/${id}/accept`),

  decline: (id: string) =>
    axiosClient.post<PurchaseRequest>(`/purchase-requests/${id}/decline`),

  getAsSeller: () =>
    axiosClient.get<PurchaseRequest[]>('/purchase-requests/as-seller'),

  getAsBuyer: () =>
    axiosClient.get<PurchaseRequest[]>('/purchase-requests/as-buyer'),
};
