import axiosClient from './axiosClient';
import type { DoctorReviewDto, CreateDoctorReviewDto } from '../types/doctorReview';

export const doctorReviewsApi = {
  getAll: (params?: { city?: string; specialization?: string; page?: number; pageSize?: number }) =>
    axiosClient.get<DoctorReviewDto[]>('/doctor-reviews', { params }).then(r => r.data),

  getById: (id: string) =>
    axiosClient.get<DoctorReviewDto>(`/doctor-reviews/${id}`).then(r => r.data),

  getMine: () =>
    axiosClient.get<DoctorReviewDto[]>('/doctor-reviews/mine').then(r => r.data),

  create: (dto: CreateDoctorReviewDto) =>
    axiosClient.post<DoctorReviewDto>('/doctor-reviews', dto).then(r => r.data),

  delete: (id: string) =>
    axiosClient.delete(`/doctor-reviews/${id}`),
};
