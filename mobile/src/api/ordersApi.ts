import axiosClient from './axiosClient';
import type { CreateSellerReviewRequest, SellerReview } from '@mamvibe/shared';

export const ordersApi = {
  confirmReceipt: (paymentId: string) =>
    axiosClient.post(`/payments/${paymentId}/confirm`),

  submitSellerReview: (paymentId: string, data: CreateSellerReviewRequest) =>
    axiosClient.post<SellerReview>(`/payments/${paymentId}/review`, data),
};
