export interface SellerReview {
  id: string;
  paymentId: string;
  reviewerId: string;
  sellerId: string;
  sellerDisplayName: string | null;
  rating: number;
  tags: string[];
  content: string;
  createdAt: string;
}

export interface CreateSellerReviewRequest {
  rating: number;
  tags: string[];
  content: string;
}
