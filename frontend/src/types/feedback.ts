export const FeedbackCategory = {
  Praise: 0,
  Improvement: 1,
  FeatureRequest: 2,
  BugReport: 3,
} as const;
export type FeedbackCategory = (typeof FeedbackCategory)[keyof typeof FeedbackCategory];

export interface Feedback {
  id: string;
  userId: string;
  userDisplayName: string | null;
  userAvatarUrl: string | null;
  rating: number;
  category: FeedbackCategory;
  content: string;
  isContactable: boolean;
  createdAt: string;
}

export interface CreateFeedbackRequest {
  rating: number;
  category: FeedbackCategory;
  content: string;
  isContactable: boolean;
}

export interface FeedbackPagedResult {
  items: Feedback[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
