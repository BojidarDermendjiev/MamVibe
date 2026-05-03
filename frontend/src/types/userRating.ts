export interface UserRating {
  id: string;
  raterId: string;
  raterDisplayName: string | null;
  raterAvatarUrl: string | null;
  ratedUserId: string;
  purchaseRequestId: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface UserRatingSummary {
  average: number | null;
  count: number;
}

export interface CreateUserRatingRequest {
  rating: number;
  comment?: string;
}
