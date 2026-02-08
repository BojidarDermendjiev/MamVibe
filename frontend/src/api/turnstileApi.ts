import axiosClient from './axiosClient';

export const turnstileApi = {
  verify: (token: string) =>
    axiosClient.post<{ verified: boolean }>('/turnstile/verify', { token }),
};
