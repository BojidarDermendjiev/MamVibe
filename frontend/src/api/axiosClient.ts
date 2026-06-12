import axios from 'axios';
import { useAuthStore } from '../store/authStore';
import { useModerationStore } from '../store/moderationStore';
import type { ModerationForbiddenEnvelope } from '../types/moderation';

function isModerationEnvelope(data: unknown): data is ModerationForbiddenEnvelope {
  if (typeof data !== 'object' || data === null) return false;
  const candidate = data as { moderation?: { level?: unknown } };
  return typeof candidate.moderation === 'object'
    && candidate.moderation !== null
    && typeof candidate.moderation.level === 'string';
}

const axiosClient = axios.create({
  // Versioned at /api/v1; the Vite proxy still matches the /api prefix so this is purely
  // a baseURL concern for the SPA. Every relative call (e.g. `client.get('/items')`)
  // resolves to /api/v1/items.
  baseURL: '/api/v1',
  withCredentials: true, // Send the httpOnly refresh-token cookie automatically
});

let isRefreshing = false;
let failedQueue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token!);
    }
  });
  failedQueue = [];
};

axiosClient.interceptors.request.use((config) => {
  // Read the short-lived access token from in-memory store (never localStorage)
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  const ALLOWED_LANGUAGES = ['en', 'bg'] as const;
  const storedLang = localStorage.getItem('language');
  const language = ALLOWED_LANGUAGES.includes(storedLang as (typeof ALLOWED_LANGUAGES)[number]) ? storedLang : 'en';
  config.headers['X-Language'] = language;

  // Set Content-Type for JSON requests only; let browser handle FormData
  if (!(config.data instanceof FormData)) {
    config.headers['Content-Type'] = 'application/json';
  }

  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Suppress 5xx error details — replace response body with a safe generic message
    // so server implementation details are never exposed in UI toasts/alerts.
    if (error.response?.status >= 500) {
      error.response.data = { error: 'A server error occurred. Please try again later.' };
    }

    // 403 with a moderation envelope means the server's UserModerationMiddleware
    // short-circuited the request. Hydrate the store so the suspension banner renders
    // immediately and the rejected call's caller sees the canonical moderation reason.
    if (error.response?.status === 403 && isModerationEnvelope(error.response.data)) {
      useModerationStore.getState().setFromEnvelope(error.response.data);
    }

    if (error.response?.status === 401 && !originalRequest._retry) {
      // If the refresh endpoint itself returned 401, the cookie is gone/invalid.
      // Do NOT attempt another refresh — that causes a double-refresh loop.
      // Silently log out; useAuth's catch block will handle UI state.
      if (originalRequest.url?.includes('/auth/refresh')) {
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
        // The httpOnly cookie is sent automatically — no token in the body
        const { data } = await axios.post('/api/v1/auth/refresh', null, { withCredentials: true });

        useAuthStore.setState({
          accessToken: data.accessToken,
          user: data.user,
          isAuthenticated: true,
        });

        axiosClient.defaults.headers.common.Authorization = `Bearer ${data.accessToken}`;
        processQueue(null, data.accessToken);
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        return axiosClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        useAuthStore.getState().logout();
        // Only hard-navigate if not already on a public auth page.
        // Navigating to /login while already there causes a full-page reload loop
        // through the Cloudflare gate.
        const publicPaths = ['/login', '/register', '/forgot-password', '/reset-password'];
        const onPublicPage = publicPaths.some((p) => window.location.pathname.startsWith(p));
        if (!onPublicPage) {
          window.location.href = '/login';
        }
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default axiosClient;
