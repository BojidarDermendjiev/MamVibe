import axiosClient from './axiosClient';
import type { Payment } from '../types/payment';
import type { PaymentDeliveryRequest } from '../types/shipping';

export const paymentsApi = {
  createCheckout: (itemId: string, delivery: PaymentDeliveryRequest) =>
    axiosClient.post<{ sessionUrl: string }>(`/payments/checkout/${itemId}`, delivery),

  createOnSpot: (itemId: string, delivery: PaymentDeliveryRequest) =>
    axiosClient.post<Payment>(`/payments/onspot/${itemId}`, delivery),

  createBooking: (itemId: string, delivery: PaymentDeliveryRequest) =>
    axiosClient.post<Payment>(`/payments/booking/${itemId}`, delivery),

  getMyPayments: () =>
    axiosClient.get<Payment[]>('/payments/my-payments'),

  createPaymentIntent: (itemId: string) =>
    axiosClient.post<{ clientSecret: string }>(`/payments/create-intent/${itemId}`),


};
