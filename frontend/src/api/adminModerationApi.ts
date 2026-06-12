import axiosClient from './axiosClient';
import type {
  AbuseReportDetail,
  ModerationActionRequest,
  PagedReportResult,
  ReportStatus,
  ReportTargetType,
  ResolveReportRequest,
  UserModerationDetail,
} from '../types/moderation';

export interface AdminReportFilter {
  status?: ReportStatus;
  targetType?: ReportTargetType;
  reason?: string;
  page?: number;
  pageSize?: number;
}

export const adminModerationApi = {
  getUserModeration: (userId: string) =>
    axiosClient.get<UserModerationDetail>(`/admin/users/${userId}/moderation`),

  applyAction: (userId: string, request: ModerationActionRequest) =>
    axiosClient.post<{ moderationLogId: string }>(`/admin/users/${userId}/moderate`, request),

  clearAction: (userId: string, reason: string) =>
    axiosClient.post(`/admin/users/${userId}/moderate/clear`, { reason }),

  getReports: (filter: AdminReportFilter = {}) =>
    axiosClient.get<PagedReportResult>('/admin/reports', { params: filter }),

  getReport: (id: string) =>
    axiosClient.get<AbuseReportDetail>(`/admin/reports/${id}`),

  resolveReport: (id: string, request: ResolveReportRequest) =>
    axiosClient.post(`/admin/reports/${id}/resolve`, request),
};
