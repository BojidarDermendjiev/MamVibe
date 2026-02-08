import axiosClient from './axiosClient';
import type { AuthResponse, LoginRequest, RegisterRequest, GoogleLoginRequest, User } from '../types/auth';

export const authApi = {
  login: (data: LoginRequest) =>
    axiosClient.post<AuthResponse>('/auth/login', data),

  register: (data: RegisterRequest) =>
    axiosClient.post<AuthResponse>('/auth/register', data),

  googleLogin: (data: GoogleLoginRequest) =>
    axiosClient.post<AuthResponse>('/auth/google', data),

  refresh: (refreshToken: string) =>
    axiosClient.post<AuthResponse>('/auth/refresh', { refreshToken }),

  revoke: () =>
    axiosClient.post('/auth/revoke'),

  me: () =>
    axiosClient.get<User>('/auth/me'),

  changePassword: (data: { currentPassword: string; newPassword: string; confirmNewPassword: string }) =>
    axiosClient.post('/auth/change-password', data),

  forgotPassword: (data: { email: string }) =>
    axiosClient.post('/auth/forgot-password', data),

  resetPassword: (data: { email: string; token: string; newPassword: string; confirmNewPassword: string }) =>
    axiosClient.post('/auth/reset-password', data),
};
