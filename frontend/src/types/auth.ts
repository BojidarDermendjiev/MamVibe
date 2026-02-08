export const ProfileType = {
  Male: 0,
  Female: 1,
  Family: 2,
} as const;
export type ProfileType = (typeof ProfileType)[keyof typeof ProfileType];

export interface User {
  id: string;
  email: string;
  displayName: string;
  profileType: ProfileType;
  avatarUrl: string | null;
  bio: string | null;
  phoneNumber: string | null;
  languagePreference: string;
  roles: string[];
  isBlocked: boolean;
  iban: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  displayName: string;
  profileType: ProfileType;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface GoogleLoginRequest {
  idToken: string;
}
