import axiosClient from "./axiosClient";
import type {
  BusinessCategory,
  CoachReferralStatus,
} from "../types/business";

// Mirrors backend BusinessProfileStatus / BusinessSubscriptionStatus / ActivityType used in the admin payloads.
export interface BusinessProfileAdminDto {
  id: string;
  userId: string;
  ownerEmail: string;
  category: BusinessCategory;
  profileKind: number;
  displayName: string;
  legalName: string;
  city: string;
  status: number; // BusinessProfileStatus
  subscriptionPlanCode: string | null;
  subscriptionStatus: number | null;
  hasListing: boolean;
  isListingApproved: boolean;
  hasDeviceConflict: boolean;
  createdAt: string;
}

export interface BusinessListingAdminDto {
  id: string;
  businessProfileId: string;
  businessDisplayName: string;
  ownerEmail: string;
  title: string;
  activityType: number;
  category: BusinessCategory;
  city: string;
  coverPhotoUrl: string | null;
  isActive: boolean;
  isApproved: boolean;
  rankBoost: number;
  viewCount: number;
  likeCount: number;
  commentCount: number;
  createdAt: string;
}

export interface BusinessRevenueDto {
  monthlyRecurringRevenueEur: number;
  activeSubscriptionCount: number;
  trialingSubscriptionCount: number;
  pastDueSubscriptionCount: number;
  canceledLast30Days: number;
  byTier: { planCode: string; activeCount: number; monthlyContributionEur: number }[];
  trialToPaidConversionRate: number;
  totalListings: number;
  approvedListings: number;
  pendingApprovalListings: number;
}

export interface PagedAdminProfiles {
  items: BusinessProfileAdminDto[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface PagedAdminListings {
  items: BusinessListingAdminDto[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CoachReferralAdminDto {
  id: string;
  businessName: string;
  contactEmail: string | null;
  contactPhone: string | null;
  activityType: number;
  city: string;
  notes: string | null;
  referrerUserId: string | null;
  referrerDisplayName: string | null;
  referralCode: string | null;
  status: CoachReferralStatus;
  adminNote: string | null;
  actionedByAdminId: string | null;
  actionedAt: string | null;
  createdAt: string;
}

export interface PagedAdminReferrals {
  items: CoachReferralAdminDto[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export const adminBusinessApi = {
  listProfiles: (params?: {
    category?: BusinessCategory;
    status?: number;
    search?: string;
    page?: number;
    pageSize?: number;
  }) =>
    axiosClient.get<PagedAdminProfiles>("/admin/business/profiles", { params }).then((r) => r.data),

  suspendProfile: (id: string, reason: string) =>
    axiosClient.post(`/admin/business/profiles/${id}/suspend`, { reason }),

  restoreProfile: (id: string) =>
    axiosClient.post(`/admin/business/profiles/${id}/restore`),

  removeProfile: (id: string, reason: string) =>
    axiosClient.post(`/admin/business/profiles/${id}/remove`, { reason }),

  listListings: (params?: {
    category?: BusinessCategory;
    isApproved?: boolean;
    isActive?: boolean;
    search?: string;
    page?: number;
    pageSize?: number;
  }) =>
    axiosClient.get<PagedAdminListings>("/admin/business/listings", { params }).then((r) => r.data),

  approveListing: (id: string) =>
    axiosClient.post(`/admin/business/listings/${id}/approve`),

  unapproveListing: (id: string, reason: string) =>
    axiosClient.post(`/admin/business/listings/${id}/unapprove`, { reason }),

  revenue: () =>
    axiosClient.get<BusinessRevenueDto>("/admin/business/revenue").then((r) => r.data),

  // Referrals (admin) — endpoint exists from Phase 7's AdminCoachReferralsController
  listReferrals: (params?: { status?: CoachReferralStatus; page?: number; pageSize?: number }) =>
    axiosClient.get<PagedAdminReferrals>("/admin/coach-referrals", { params }).then((r) => r.data),

  updateReferralStatus: (id: string, status: CoachReferralStatus, adminNote?: string) =>
    axiosClient.post(`/admin/coach-referrals/${id}/status`, { status, adminNote }),
};
