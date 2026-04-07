import axiosClient from './axiosClient';
import type { EBill } from '../types/ebill';

export const ebillsApi = {
  getMyEBills: () =>
    axiosClient.get<EBill[]>('/ebills'),

  getEBill: (id: string) =>
    axiosClient.get<EBill>(`/ebills/${id}`),

  resendEmail: (id: string) =>
    axiosClient.post(`/ebills/${id}/resend`),
};
