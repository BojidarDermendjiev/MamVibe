import axiosClient from './axiosClient';
import type { CreateFeedbackRequest, FeedbackPagedResult } from '../types/feedback';

export const feedbackApi = {
  getAll: (page = 1, pageSize = 10) =>
    axiosClient.get<FeedbackPagedResult>('/feedback', { params: { page, pageSize } }),

  create: (data: CreateFeedbackRequest) =>
    axiosClient.post('/feedback', data),

  delete: (id: string) =>
    axiosClient.delete(`/feedback/${id}`),
};
