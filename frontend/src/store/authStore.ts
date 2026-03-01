import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User } from '../types/auth';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  setAuth: (user: User, accessToken: string) => void;
  setUser: (user: User) => void;
  logout: () => void;
  setLoading: (loading: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,      // Memory only — never persisted to localStorage
      isAuthenticated: false, // Resolved by silent refresh on app load
      isLoading: true,
      setAuth: (user, accessToken) => {
        set({ user, accessToken, isAuthenticated: true, isLoading: false });
      },
      setUser: (user) => set({ user }),
      logout: () => {
        set({ user: null, accessToken: null, isAuthenticated: false, isLoading: false });
      },
      setLoading: (loading) => set({ isLoading: loading }),
    }),
    {
      name: 'mamvibe-auth',
      // Only persist the user profile — access token stays memory-only for security.
      // The refresh token lives in an httpOnly cookie (set by the backend).
      partialize: (s) => ({ user: s.user }),
      onRehydrateStorage: () => (state) => {
        if (state?.user) {
          // Persisted user found — optimistically mark as authenticated so
          // protected routes and the navbar render immediately without a spinner.
          // useAuth will silently refresh the access token in the background;
          // if the cookie is expired/revoked, logout() clears this state.
          state.isAuthenticated = true;
        }
      },
    },
  ),
);
