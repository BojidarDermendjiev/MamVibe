import axiosClient from './axiosClient';
import type { FollowToggleResult, FollowStatus, FollowUserDto } from '../types/follow';
import type { PagedResult } from '../types/item';
import type { Item } from '../types/item';

export const followsApi = {
  toggle: (userId: string) =>
    axiosClient.post<FollowToggleResult>(`/follows/${userId}`),

  getStatus: (userId: string) =>
    axiosClient.get<FollowStatus>(`/follows/${userId}/status`),

  getFollowing: () =>
    axiosClient.get<FollowUserDto[]>('/follows/following'),

  getFollowers: () =>
    axiosClient.get<FollowUserDto[]>('/follows/followers'),

  getFeed: (page = 1, pageSize = 12) =>
    axiosClient.get<PagedResult<Item>>('/follows/feed', { params: { page, pageSize } }),
};
