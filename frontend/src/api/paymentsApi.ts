import axiosClient from './axiosClient';
import type { Payment } from '../types/payment';
import type { PagedResult } from '../types/item';
import type { PaymentDeliveryRequest } from '../types/shipping';

/**
 * Wraps an optional client-supplied `Idempotency-Key` into Axios request config.
 * The backend uses this key to dedupe duplicate payment-creation requests
 * (double-taps, retries) at both the DB unique-index level and (for Stripe-backed
 * methods) at the Stripe API level via `RequestOptions.IdempotencyKey`.
 *
 * Callers should generate the key ONCE per logical purchase attempt — e.g.
 * `const key = crypto.randomUUID()` at button-press time — and reuse it across
 * retries so the backend recognises them as duplicates.
 */
const idempotencyConfig = (idempotencyKey?: string) =>
  idempotencyKey ? { headers: { 'Idempotency-Key': idempotencyKey } } : undefined;

export const paymentsApi = {
  createCheckout: (itemId: string, delivery: PaymentDeliveryRequest, idempotencyKey?: string) =>
    axiosClient.post<{ sessionUrl: string }>(`/payments/checkout/${itemId}`, delivery, idempotencyConfig(idempotencyKey)),

  createOnSpot: (itemId: string, delivery: PaymentDeliveryRequest, idempotencyKey?: string) =>
    axiosClient.post<Payment>(`/payments/onspot/${itemId}`, delivery, idempotencyConfig(idempotencyKey)),

  createBooking: (itemId: string, delivery: PaymentDeliveryRequest, idempotencyKey?: string) =>
    axiosClient.post<Payment>(`/payments/booking/${itemId}`, delivery, idempotencyConfig(idempotencyKey)),

  createCod: (itemId: string, delivery: PaymentDeliveryRequest, idempotencyKey?: string) =>
    axiosClient.post<Payment>(`/payments/cod/${itemId}`, delivery, idempotencyConfig(idempotencyKey)),

  getMyPayments: () =>
    axiosClient.get<PagedResult<Payment>>('/payments/my-payments'),

  createPaymentIntent: (itemId: string, idempotencyKey?: string) =>
    axiosClient.post<{ clientSecret: string }>(`/payments/create-intent/${itemId}`, null, idempotencyConfig(idempotencyKey)),

  createDonationCheckout: (amount: number, idempotencyKey?: string) =>
    axiosClient.post<{ sessionUrl: string }>('/payments/donation/checkout', { amount }, idempotencyConfig(idempotencyKey)),
};
