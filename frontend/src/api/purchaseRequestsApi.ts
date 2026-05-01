import axiosClient from './axiosClient';
import type { PurchaseRequest } from '../types/purchaseRequest';

export const purchaseRequestsApi = {
  create: (itemId: string) =>
    axiosClient.post<PurchaseRequest>('/purchase-requests', { itemId }),

  accept: (id: string) =>
    axiosClient.post<PurchaseRequest>(`/purchase-requests/${id}/accept`),

  decline: (id: string) =>
    axiosClient.post<PurchaseRequest>(`/purchase-requests/${id}/decline`),

  paymentChosen: (id: string, paymentMethod: string) =>
    axiosClient.post<PurchaseRequest>(`/purchase-requests/${id}/payment-chosen`, { paymentMethod }),

  getAsSeller: () =>
    axiosClient.get<PurchaseRequest[]>('/purchase-requests/as-seller'),

  getAsBuyer: () =>
    axiosClient.get<PurchaseRequest[]>('/purchase-requests/as-buyer'),

  checkBuyer: (id: string) =>
    axiosClient.get<BuyerCheckResult>(`/purchase-requests/${id}/buyer-check`),
};

export interface NekorektenReport {
  text?: string;
  phone?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  likes: number;
  createdAt?: string;
}

export interface BuyerCheckResult {
  hasReports: boolean;
  reportCount: number;
  reports: NekorektenReport[];
  serviceUnavailable: boolean;
}

/** PII-free report entry returned by the seller-check endpoint. */
export interface SellerCheckReport {
  text?: string;
  likes: number;
  createdAt?: string;
}

/** Result of GET /api/items/{id}/seller-check — no PII fields. */
export interface SellerCheckResult {
  hasReports: boolean;
  reportCount: number;
  reports: SellerCheckReport[];
  serviceUnavailable: boolean;
}
