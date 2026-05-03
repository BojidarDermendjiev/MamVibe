import axiosClient from './axiosClient';
import type { UserRating, UserRatingSummary, CreateUserRatingRequest } from '../types/userRating';

export const userRatingsApi = {
  create: (purchaseRequestId: string, data: CreateUserRatingRequest) =>
    axiosClient.post<UserRating>(`/purchase-requests/${purchaseRequestId}/rating`, data),

  getForUser: (userId: string) =>
    axiosClient.get<UserRating[]>(`/users/${userId}/ratings`),

  getSummary: (userId: string) =>
    axiosClient.get<UserRatingSummary>(`/users/${userId}/ratings/summary`),
};
