import axiosClient from './axiosClient';
import type { SubmitReportRequest } from '../types/moderation';

export const reportsApi = {
  submit: (request: SubmitReportRequest) =>
    axiosClient.post<{ id: string }>('/reports', request),
};
