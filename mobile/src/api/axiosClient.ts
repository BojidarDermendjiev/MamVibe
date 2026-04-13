import axios from 'axios';
import * as SecureStore from 'expo-secure-store';
import { useAuthStore } from '@/store/authStore';

const API_URL = process.env.EXPO_PUBLIC_API_URL ?? 'http://10.0.2.2:5038/api';

/** Base server URL (no /api suffix) — used for building absolute media URLs */
export const SERVER_URL = API_URL.replace(/\/api\/?$/, '');

const axiosClient = axios.create({
  baseURL: API_URL,
});

const TOKEN_KEY = 'mamvibe_access_token';
const REFRESH_TOKEN_KEY = 'mamvibe_refresh_token';

export const tokenStorage = {
  getAccessToken: () => SecureStore.getItemAsync(TOKEN_KEY),
  setAccessToken: (token: string) => SecureStore.setItemAsync(TOKEN_KEY, token),
  getRefreshToken: () => SecureStore.getItemAsync(REFRESH_TOKEN_KEY),
  setRefreshToken: (token: string) => SecureStore.setItemAsync(REFRESH_TOKEN_KEY, token),
  clearTokens: async () => {
    await SecureStore.deleteItemAsync(TOKEN_KEY);
    await SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY);
  },
};

let isRefreshing = false;
let failedQueue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((p) => (error ? p.reject(error) : p.resolve(token!)));
  failedQueue = [];
};

axiosClient.interceptors.request.use(async (config) => {
  const token = await tokenStorage.getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  if (!(config.data instanceof FormData)) {
    config.headers['Content-Type'] = 'application/json';
  }
  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status >= 500) {
      error.response.data = { error: 'A server error occurred. Please try again later.' };
    }

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (originalRequest.url?.includes('/auth/refresh')) {
        await tokenStorage.clearTokens();
        useAuthStore.getState().logout();
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return axiosClient(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = await tokenStorage.getRefreshToken();
        // Send refreshToken in the request body so native mobile clients work
        // (httpOnly cookies set by the server are not automatically sent by Axios
        // on all React Native versions; the body is a reliable fallback).
        const { data } = await axios.post(`${API_URL}/auth/refresh`, { refreshToken });

        await tokenStorage.setAccessToken(data.accessToken);
        if (data.refreshToken) {
          await tokenStorage.setRefreshToken(data.refreshToken);
        }

        axiosClient.defaults.headers.common.Authorization = `Bearer ${data.accessToken}`;
        processQueue(null, data.accessToken);
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        return axiosClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        await tokenStorage.clearTokens();
        // Clear auth state so the app navigates back to the login screen.
        useAuthStore.getState().logout();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  },
);

export default axiosClient;
