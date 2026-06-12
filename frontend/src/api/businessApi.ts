import axiosClient from "./axiosClient";
import type {
  BrowseListingsResult,
  BusinessCategory,
  BusinessListingCommentDto,
  BusinessListingDto,
  BusinessPolicyDto,
  BusinessProfileDto,
  BusinessSubscriptionDto,
  CreateBusinessListingCommentRequest,
  CreateBusinessListingRequest,
  CreateBusinessProfileRequest,
  ListingLikeStateDto,
  PagedCommentsResult,
  PromoterDashboardDto,
  PromoterProfileDto,
  ReportBusinessListingRequest,
  SubmitCoachReferralRequest,
  SubscriptionPlanDto,
  UpdateBusinessListingRequest,
  UpdateBusinessProfileRequest,
} from "../types/business";
import type { ActivityType } from "../types/business";

export const businessApi = {
  // Policy
  getCurrentPolicy: (language?: string) =>
    axiosClient
      .get<BusinessPolicyDto>("/business/policy/current", { params: { language } })
      .then((r) => r.data),

  acceptPolicy: (policyVersionId: string) =>
    axiosClient.post("/business/policy/accept", { policyVersionId }),

  // Profile
  getMyProfile: () =>
    axiosClient
      .get<BusinessProfileDto>("/business/profile/me")
      .then((r) => r.data),

  createProfile: (request: CreateBusinessProfileRequest) =>
    axiosClient
      .post<BusinessProfileDto>("/business/profile", request)
      .then((r) => r.data),

  updateProfile: (request: UpdateBusinessProfileRequest) =>
    axiosClient
      .put<BusinessProfileDto>("/business/profile", request)
      .then((r) => r.data),

  // Listings — public browse + auth CRUD
  browseListings: (params?: {
    category?: BusinessCategory;
    city?: string;
    activityType?: ActivityType;
    ageMonths?: number;
    page?: number;
    pageSize?: number;
  }) =>
    axiosClient
      .get<BrowseListingsResult>("/business/listings", { params })
      .then((r) => r.data),

  getListingById: (id: string) =>
    axiosClient
      .get<BusinessListingDto>(`/business/listings/${id}`)
      .then((r) => r.data),

  getMyListing: () =>
    axiosClient
      .get<BusinessListingDto>("/business/listings/me")
      .then((r) => r.data),

  createListing: (request: CreateBusinessListingRequest) =>
    axiosClient
      .post<BusinessListingDto>("/business/listings", request)
      .then((r) => r.data),

  updateListing: (id: string, request: UpdateBusinessListingRequest) =>
    axiosClient
      .put<BusinessListingDto>(`/business/listings/${id}`, request)
      .then((r) => r.data),

  deleteListing: (id: string) =>
    axiosClient.delete(`/business/listings/${id}`),

  // Photo upload — reuses the existing /photos/upload endpoint.
  uploadPhoto: async (file: File): Promise<string> => {
    const fd = new FormData();
    fd.append("file", file);
    const res = await axiosClient.post<{ url: string }>("/photos/upload", fd, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    return res.data.url;
  },

  // Interactions — likes
  likeListing: (id: string) =>
    axiosClient
      .post<ListingLikeStateDto>(`/business/listings/${id}/like`)
      .then((r) => r.data),

  unlikeListing: (id: string) =>
    axiosClient
      .delete<ListingLikeStateDto>(`/business/listings/${id}/like`)
      .then((r) => r.data),

  // Interactions — comments
  getListingComments: (id: string, page = 1, pageSize = 20) =>
    axiosClient
      .get<PagedCommentsResult>(`/business/listings/${id}/comments`, {
        params: { page, pageSize },
      })
      .then((r) => r.data),

  postListingComment: (id: string, request: CreateBusinessListingCommentRequest) =>
    axiosClient
      .post<BusinessListingCommentDto>(`/business/listings/${id}/comments`, request)
      .then((r) => r.data),

  deleteListingComment: (listingId: string, commentId: string) =>
    axiosClient.delete(`/business/listings/${listingId}/comments/${commentId}`),

  // Interactions — report
  reportListing: (id: string, request: ReportBusinessListingRequest) =>
    axiosClient.post(`/business/listings/${id}/report`, request),

  // Interactions — view tracking (anonymous-safe)
  trackListingView: (id: string) =>
    axiosClient.post(`/business/listings/${id}/view`),

  // Promoter
  getMyPromoterProfile: () =>
    axiosClient.get<PromoterProfileDto>("/promoter/me").then((r) => r.data),

  createPromoterProfile: () =>
    axiosClient.post<PromoterProfileDto>("/promoter").then((r) => r.data),

  getPromoterDashboard: () =>
    axiosClient.get<PromoterDashboardDto>("/promoter/dashboard").then((r) => r.data),

  // Coach referrals — public submission
  submitCoachReferral: (request: SubmitCoachReferralRequest) =>
    axiosClient
      .post<{ id: string }>("/coach-referrals", request)
      .then((r) => r.data),

  // Subscriptions
  getSubscriptionPlans: () =>
    axiosClient
      .get<SubscriptionPlanDto[]>("/business/subscription/plans")
      .then((r) => r.data),

  getMySubscription: () =>
    axiosClient
      .get<BusinessSubscriptionDto>("/business/subscription/me")
      .then((r) => r.data),

  createSubscriptionCheckout: (planCode: string, successUrl: string, cancelUrl: string) =>
    axiosClient
      .post<{ url: string }>("/business/subscription/checkout", {
        planCode,
        successUrl,
        cancelUrl,
      })
      .then((r) => r.data.url),

  createBillingPortalUrl: (returnUrl: string) =>
    axiosClient
      .post<{ url: string }>("/business/subscription/portal", { returnUrl })
      .then((r) => r.data.url),

  cancelSubscription: (atPeriodEnd = true) =>
    axiosClient.post(`/business/subscription/cancel?atPeriodEnd=${atPeriodEnd}`),
};
