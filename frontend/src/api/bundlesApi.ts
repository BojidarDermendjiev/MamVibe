import axiosClient from './axiosClient';
import type { BundleDto, CreateBundleDto } from '../types/bundle';
import type { PaymentDeliveryRequest } from '../types/shipping';

export const bundlesApi = {
  getById: (id: string) =>
    axiosClient.get<BundleDto>(`/bundles/${id}`),

  getMy: () =>
    axiosClient.get<BundleDto[]>('/bundles/my'),

  create: (dto: CreateBundleDto) =>
    axiosClient.post<BundleDto>('/bundles', dto),

  delete: (id: string) =>
    axiosClient.delete(`/bundles/${id}`),

  requestPurchase: (id: string) =>
    axiosClient.post(`/bundles/${id}/request`),

  createCheckout: (id: string, delivery: PaymentDeliveryRequest | null) =>
    axiosClient.post<{ url: string }>(`/bundles/${id}/payment/checkout`, delivery),

  createOnSpot: (id: string, delivery: PaymentDeliveryRequest | null) =>
    axiosClient.post(`/bundles/${id}/payment/on-spot`, delivery),

  createCod: (id: string, delivery: PaymentDeliveryRequest) =>
    axiosClient.post(`/bundles/${id}/payment/cod`, delivery),
};
