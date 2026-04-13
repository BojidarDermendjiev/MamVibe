import axiosClient from './axiosClient';

export const authApi = {
  updateProfile: (data: { displayName?: string; bio?: string; iban?: string }) =>
    axiosClient.put('/users/profile', data),

  changePassword: (data: { currentPassword: string; newPassword: string; confirmNewPassword: string }) =>
    axiosClient.post('/auth/change-password', data),
};
