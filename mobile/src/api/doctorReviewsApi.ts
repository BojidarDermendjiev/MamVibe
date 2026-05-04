import axiosClient from './axiosClient';

export interface DoctorReviewDto {
  id: string;
  userId: string;
  authorDisplayName: string | null;
  authorAvatarUrl: string | null;
  doctorName: string;
  specialization: string;
  clinicName: string | null;
  city: string;
  rating: number;
  content: string;
  superdocUrl: string | null;
  isAnonymous: boolean;
  isApproved: boolean;
  createdAt: string;
}

export interface CreateDoctorReviewDto {
  doctorName: string;
  specialization: string;
  clinicName?: string;
  city: string;
  rating: number;
  content: string;
  superdocUrl?: string;
  isAnonymous: boolean;
}

export const doctorReviewsApi = {
  getAll: () =>
    axiosClient.get<DoctorReviewDto[]>('/doctor-reviews').then((r) => r.data),

  create: (dto: CreateDoctorReviewDto) =>
    axiosClient.post<DoctorReviewDto>('/doctor-reviews', dto).then((r) => r.data),

  delete: (id: string) =>
    axiosClient.delete(`/doctor-reviews/${id}`),

  getPending: () =>
    axiosClient.get<DoctorReviewDto[]>('/admin/doctor-reviews/pending').then((r) => r.data),

  approve: (id: string) =>
    axiosClient.post(`/admin/doctor-reviews/${id}/approve`),

  adminDelete: (id: string) =>
    axiosClient.delete(`/admin/doctor-reviews/${id}`),
};
