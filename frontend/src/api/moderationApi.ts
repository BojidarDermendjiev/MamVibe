import axiosClient from './axiosClient';
import type { UserModerationStatus } from '../types/moderation';

export const moderationApi = {
  /**
   * Returns the current user's own moderation snapshot — drives the suspension banner.
   * Safe to call even when the user has no active moderation (returns level=None).
   */
  getMyStatus: () => axiosClient.get<UserModerationStatus>('/users/me/moderation-status'),
};
