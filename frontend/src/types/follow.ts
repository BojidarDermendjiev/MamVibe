export interface FollowUserDto {
  id: string;
  displayName: string;
  avatarUrl: string | null;
  isOnHoliday: boolean;
  followerCount: number;
  itemCount: number;
  followedAt: string;
}

export interface FollowToggleResult {
  isFollowing: boolean;
  followerCount: number;
}

export interface FollowStatus {
  isFollowing: boolean;
  followerCount: number;
}

export interface NewFollowerNotification {
  followerId: string;
  followerDisplayName: string;
  followerAvatarUrl: string | null;
  followedAt: string;
}
