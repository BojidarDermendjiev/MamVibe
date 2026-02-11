import axiosClient from './axiosClient';
import type { Payment } from '../types/payment';

export const paymentsApi = {
  createCheckout: (itemId: string) =>
    axiosClient.post<{ sessionUrl: string }>(`/payments/checkout/${itemId}`),

  createOnSpot: (itemId: string) =>
    axiosClient.post<Payment>(`/payments/onspot/${itemId}`),

  createBooking: (itemId: string) =>
    axiosClient.post<Payment>(`/payments/booking/${itemId}`),

  getMyPayments: () =>
    axiosClient.get<Payment[]>('/payments/my-payments'),

  createPaymentIntent: (itemId: string) =>
    axiosClient.post<{ clientSecret: string }>(`/payments/create-intent/${itemId}`),

  bulkCheckout: (itemIds: string[]) =>
    axiosClient.post<{ sessionUrl: string }>('/payments/bulk-checkout', { itemIds }),

  bulkBooking: (itemIds: string[]) =>
    axiosClient.post<Payment[]>('/payments/bulk-booking', { itemIds }),

  bulkOnSpot: (itemIds: string[]) =>
    axiosClient.post<Payment[]>('/payments/bulk-onspot', { itemIds }),
};
