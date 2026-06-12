// Mirrors backend `BusinessCategory` enum.
export const BusinessCategory = {
  Coach: 0,
  VenueAdvertiser: 1,
} as const;
export type BusinessCategory = (typeof BusinessCategory)[keyof typeof BusinessCategory];

// Mirrors backend `ProfileKind` enum (numeric values).
export const ProfileKind = {
  Coach: 0,
  Agency: 1,
} as const;
export type ProfileKind = (typeof ProfileKind)[keyof typeof ProfileKind];

// Mirrors backend `BusinessSubscriptionStatus` enum.
export const BusinessSubscriptionStatus = {
  Incomplete: 0,
  Trialing: 1,
  Active: 2,
  PastDue: 3,
  Canceled: 4,
} as const;
export type BusinessSubscriptionStatus =
  (typeof BusinessSubscriptionStatus)[keyof typeof BusinessSubscriptionStatus];

export interface BusinessSubscriptionDto {
  id: string;
  businessProfileId: string;
  planCode: string;
  planDisplayName: string;
  monthlyPriceEur: number;
  rankBoost: number;
  status: BusinessSubscriptionStatus;
  currentPeriodStart: string | null;
  currentPeriodEnd: string | null;
  trialEndsAt: string | null;
  gracePeriodEndsAt: string | null;
  canceledAt: string | null;
  hasStripeSubscription: boolean;
}

export interface SubscriptionPlanDto {
  code: string;
  displayName: string;
  monthlyPriceEur: number;
  rankBoost: number;
  trialDays: number;
  featuresJson: string | null;
  sortOrder: number;
  isCheckoutEnabled: boolean;
}

// Mirrors backend `BusinessProfileStatus` enum.
export const BusinessProfileStatus = {
  PendingPolicy: 0,
  PendingPayment: 1,
  Active: 2,
  PastDue: 3,
  Suspended: 4,
  Removed: 5,
} as const;
export type BusinessProfileStatus =
  (typeof BusinessProfileStatus)[keyof typeof BusinessProfileStatus];

// Mirrors backend `ActivityType` enum.
export const ActivityType = {
  Swimming: 0,
  MartialArts: 1,
  Music: 2,
  Dance: 3,
  Gymnastics: 4,
  ArtAndCrafts: 5,
  EarlyDevelopment: 6,
  LanguageClasses: 7,
  SportsTeam: 8,
  Other: 99,
} as const;
export type ActivityType = (typeof ActivityType)[keyof typeof ActivityType];

export interface BusinessPolicyDto {
  id: string;
  version: number;
  language: string;
  title: string;
  bodyMarkdown: string;
  effectiveFrom: string;
}

export interface BusinessProfileDto {
  id: string;
  userId: string;
  category: BusinessCategory;
  profileKind: ProfileKind;
  legalName: string;
  displayName: string;
  bio: string | null;
  contactEmail: string;
  contactPhone: string | null;
  website: string | null;
  city: string;
  status: BusinessProfileStatus;
  policyReacceptanceRequired: boolean;
  hasListing: boolean;
  hasSubscription: boolean;
  createdAt: string;
}

export interface CreateBusinessProfileRequest {
  category: BusinessCategory;
  profileKind: ProfileKind;
  legalName: string;
  displayName: string;
  bio?: string;
  contactEmail: string;
  contactPhone?: string;
  website?: string;
  city: string;
  policyVersionId: string;
  fingerprintVisitorId: string;
}

export interface UpdateBusinessProfileRequest {
  profileKind: ProfileKind;
  legalName: string;
  displayName: string;
  bio?: string;
  contactEmail: string;
  contactPhone?: string;
  website?: string;
  city: string;
}

// Stable client-facing codes returned by the backend's coded exceptions (403/409 bodies
// carry `code` alongside `error`). Used by the registration UX to show targeted messages.
export type BusinessErrorCode =
  | "profile_already_exists"
  | "policy_outdated"
  | "fingerprint_missing"
  | "device_already_has_business";

export interface BusinessErrorEnvelope {
  error: string;
  code?: BusinessErrorCode;
  statusCode: number;
}

export interface BusinessListingPhotoDto {
  id: string;
  url: string;
  displayOrder: number;
  isCover: boolean;
}

export interface BusinessListingSummaryDto {
  id: string;
  title: string;
  activityType: ActivityType;
  city: string;
  ageFromMonths: number | null;
  ageToMonths: number | null;
  priceFromEur: number | null;
  coverPhotoUrl: string | null;
  businessDisplayName: string;
  rankBoost: number;
  likeCount: number;
  commentCount: number;
  createdAt: string;
}

export interface BusinessListingDto {
  id: string;
  businessProfileId: string;
  title: string;
  description: string;
  activityType: ActivityType;
  city: string;
  addressLine: string | null;
  latitude: number | null;
  longitude: number | null;
  ageFromMonths: number | null;
  ageToMonths: number | null;
  priceFromEur: number | null;
  schedule: string | null;
  isActive: boolean;
  isApproved: boolean;
  rankBoost: number;
  viewCount: number;
  likeCount: number;
  commentCount: number;
  photos: BusinessListingPhotoDto[];
  businessDisplayName: string;
  businessBio: string | null;
  businessContactEmail: string;
  businessContactPhone: string | null;
  businessWebsite: string | null;
  createdAt: string;
  updatedAt: string | null;
  isLikedByCurrentUser: boolean;
}

export interface ListingLikeStateDto {
  isLiked: boolean;
  likeCount: number;
}

export interface BusinessListingCommentDto {
  id: string;
  listingId: string;
  userId: string;
  authorDisplayName: string;
  authorAvatarUrl: string | null;
  body: string;
  parentCommentId: string | null;
  isHidden: boolean;
  hiddenReason: string | null;
  createdAt: string;
}

export interface PagedCommentsResult {
  items: BusinessListingCommentDto[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateBusinessListingCommentRequest {
  body: string;
  parentCommentId?: string;
}

// Subset of backend `ModerationReason` exposed to parents reporting a listing.
export const ListingReportReason = {
  Spam: 1,
  Scam: 2,
  Harassment: 3,
  FakeListing: 4,
  Inappropriate: 5,
  Other: 13,
} as const;
export type ListingReportReason =
  (typeof ListingReportReason)[keyof typeof ListingReportReason];

export interface ReportBusinessListingRequest {
  reason: ListingReportReason;
  description: string;
}

// Mirrors backend `CoachReferralStatus` enum.
export const CoachReferralStatus = {
  Submitted: 0,
  Contacted: 1,
  Onboarded: 2,
  Rejected: 3,
} as const;
export type CoachReferralStatus =
  (typeof CoachReferralStatus)[keyof typeof CoachReferralStatus];

export interface PromoterProfileDto {
  id: string;
  userId: string;
  referralCode: string;
  isActive: boolean;
  totalReferrals: number;
  totalActivations: number;
  createdAt: string;
}

export interface RecentReferralDto {
  id: string;
  businessName: string;
  city: string;
  activityType: ActivityType;
  status: CoachReferralStatus;
  createdAt: string;
}

export interface PromoterDashboardDto {
  profile: PromoterProfileDto;
  totalSubmitted: number;
  totalContacted: number;
  totalOnboarded: number;
  totalRejected: number;
  recent: RecentReferralDto[];
}

export interface SubmitCoachReferralRequest {
  businessName: string;
  contactEmail?: string;
  contactPhone?: string;
  activityType: ActivityType;
  city: string;
  notes?: string;
  referralCode?: string;
  turnstileToken?: string;
}

export interface BrowseListingsResult {
  featured: BusinessListingSummaryDto[];
  items: BusinessListingSummaryDto[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateBusinessListingRequest {
  title: string;
  description: string;
  activityType: ActivityType;
  city: string;
  addressLine?: string;
  latitude?: number;
  longitude?: number;
  ageFromMonths?: number;
  ageToMonths?: number;
  priceFromEur?: number;
  schedule?: string;
  photoUrls: string[];
}

export interface UpdateBusinessListingRequest extends CreateBusinessListingRequest {
  isActive: boolean;
}
