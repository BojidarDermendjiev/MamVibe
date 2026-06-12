import axiosClient from './axiosClient';
import type {
  Appeal,
  DecideAppealRequest,
  PagedAppealResult,
  SubmitAppealRequest,
} from '../types/moderation';

export const appealsApi = {
  submit: (request: SubmitAppealRequest) =>
    axiosClient.post<{ id: string }>('/users/me/appeals', request),

  listMine: () => axiosClient.get<Appeal[]>('/users/me/appeals'),
};

export const adminAppealsApi = {
  list: (params: { status?: string; page?: number; pageSize?: number } = {}) =>
    axiosClient.get<PagedAppealResult>('/admin/appeals', { params }),

  get: (id: string) => axiosClient.get<Appeal>(`/admin/appeals/${id}`),

  decide: (id: string, request: DecideAppealRequest) =>
    axiosClient.post(`/admin/appeals/${id}/decide`, request),
};
