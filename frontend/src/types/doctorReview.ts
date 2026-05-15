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
}
