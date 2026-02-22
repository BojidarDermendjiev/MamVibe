import { create } from 'zustand';
import type { User } from '../types/auth';
import { useCartStore } from './cartStore';

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

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  accessToken: null,      // Memory only — never persisted to localStorage
  isAuthenticated: false, // Resolved by silent refresh on app load
  isLoading: true,
  setAuth: (user, accessToken) => {
    set({ user, accessToken, isAuthenticated: true, isLoading: false });
  },
  setUser: (user) => set({ user }),
  logout: () => {
    // Clear persisted cart data so it doesn't leak to the next session
    useCartStore.getState().clearCart();
    set({ user: null, accessToken: null, isAuthenticated: false, isLoading: false });
  },
  setLoading: (loading) => set({ isLoading: loading }),
}));
