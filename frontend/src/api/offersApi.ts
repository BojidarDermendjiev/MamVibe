import axiosClient from './axiosClient';
import type { Offer } from '../types/offer';

export const offersApi = {
  create: (itemId: string, offeredPrice: number) =>
    axiosClient.post<Offer>('/offers', { itemId, offeredPrice }),

  accept: (id: string) =>
    axiosClient.post<Offer>(`/offers/${id}/accept`),

  decline: (id: string) =>
    axiosClient.post<Offer>(`/offers/${id}/decline`),

  counter: (id: string, counterPrice: number) =>
    axiosClient.post<Offer>(`/offers/${id}/counter`, { counterPrice }),

  acceptCounter: (id: string) =>
    axiosClient.post<Offer>(`/offers/${id}/accept-counter`),

  declineCounter: (id: string) =>
    axiosClient.post<Offer>(`/offers/${id}/decline-counter`),

  cancel: (id: string) =>
    axiosClient.post<Offer>(`/offers/${id}/cancel`),

  getReceived: () =>
    axiosClient.get<Offer[]>('/offers/received'),

  getSent: () =>
    axiosClient.get<Offer[]>('/offers/sent'),
};
