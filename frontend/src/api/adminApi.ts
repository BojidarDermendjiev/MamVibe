import axiosClient from './axiosClient';
import type { Item } from '../types/item';
import type { Payment } from '../types/payment';
import type { Shipment, TrackingEvent } from '../types/shipping';

export interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  profileType: number;
  avatarUrl: string | null;
  isBlocked: boolean;
  roles: string[];
  createdAt: string;
}

export interface DashboardStats {
  totalUsers: number;
  totalItems: number;
  activeItems: number;
  totalDonations: number;
  totalSales: number;
  totalRevenue: number;
  totalMessages: number;
  blockedUsers: number;
}

export const adminApi = {
  getDashboard: () =>
    axiosClient.get<DashboardStats>('/admin/dashboard'),

  getUsers: (search?: string) =>
    axiosClient.get<{ items: AdminUser[]; totalCount: number }>('/admin/users', { params: { search } }),

  blockUser: (userId: string) =>
    axiosClient.post(`/admin/users/${userId}/block`),

  unblockUser: (userId: string) =>
    axiosClient.post(`/admin/users/${userId}/unblock`),

  deleteItem: (itemId: string) =>
    axiosClient.delete(`/admin/items/${itemId}`),

  // Feature 2: Item approval
  getPendingItems: () =>
    axiosClient.get<Item[]>('/admin/items/pending'),

  approveItem: (id: string) =>
    axiosClient.post(`/admin/items/${id}/approve`),

  // Feature 3: Admin shipping & payments
  getAllShipments: () =>
    axiosClient.get<Shipment[]>('/admin/shipments'),

  getAllPayments: () =>
    axiosClient.get<Payment[]>('/admin/payments'),

  trackShipment: (id: string) =>
    axiosClient.get<TrackingEvent[]>(`/admin/shipments/${id}/track`),

  getAiSettings: () =>
    axiosClient.get<{ model: string; availableModels: string[] }>('/admin/ai-settings'),

  updateAiSettings: (model: string) =>
    axiosClient.put('/admin/ai-settings', { model }),
};
