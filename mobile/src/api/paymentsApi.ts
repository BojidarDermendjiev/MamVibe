import axiosClient from './axiosClient';
import type { Payment } from '@mamvibe/shared';
import type { PaymentDeliveryRequest } from '@mamvibe/shared';

export const paymentsApi = {
  createOnSpot: (itemId: string, delivery: PaymentDeliveryRequest) =>
    axiosClient.post<Payment>(`/payments/onspot/${itemId}`, delivery),

  createBooking: (itemId: string, delivery: PaymentDeliveryRequest) =>
    axiosClient.post<Payment>(`/payments/booking/${itemId}`, delivery),

  createPaymentIntent: (itemId: string) =>
    axiosClient.post<{ clientSecret: string }>(`/payments/create-intent/${itemId}`),

  getMyPayments: () =>
    axiosClient.get<Payment[]>('/payments/my-payments'),

  createDonationIntent: (amount: number) =>
    axiosClient.post<{ clientSecret: string }>('/payments/donation/intent', { amount }),
};
