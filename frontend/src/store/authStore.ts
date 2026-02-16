import { create } from 'zustand';
import type { User } from '../types/auth';
import { useCartStore } from './cartStore';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  setAuth: (user: User, accessToken: string, refreshToken: string) => void;
  setUser: (user: User) => void;
  logout: () => void;
  setLoading: (loading: boolean) => void;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  accessToken: localStorage.getItem('accessToken'),
  refreshToken: localStorage.getItem('refreshToken'),
  isAuthenticated: !!localStorage.getItem('accessToken'),
  isLoading: true,
  setAuth: (user, accessToken, refreshToken) => {
    // Clear cart if a different user logs in
    const currentUser = get().user;
    if (currentUser && currentUser.id !== user.id) {
      useCartStore.getState().clearCart();
    }
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    set({ user, accessToken, refreshToken, isAuthenticated: true, isLoading: false });
  },
  setUser: (user) => set({ user }),
  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    useCartStore.getState().clearCart();
    set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false, isLoading: false });
  },
  setLoading: (loading) => set({ isLoading: loading }),
}));
