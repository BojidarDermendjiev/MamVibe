import axiosClient from './axiosClient';

export interface PublicStats {
  activeListings: number;
  totalSellers: number;
  happyFamilies: number;
}

export const statsApi = {
  getPublic: () => axiosClient.get<PublicStats>('/stats').then((r) => r.data),
};
