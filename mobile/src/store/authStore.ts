import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import AsyncStorage from '@react-native-async-storage/async-storage';
import type { User } from '@mamvibe/shared';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  setAuth: (user: User, accessToken: string, refreshToken?: string) => void;
  setUser: (user: User) => void;
  logout: () => void;
  setLoading: (loading: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,
      isLoading: true,
      setAuth: (user, _accessToken, _refreshToken) => {
        // Tokens are stored in SecureStore by axiosClient — not here
        set({ user, isAuthenticated: true, isLoading: false });
      },
      setUser: (user) => set({ user }),
      logout: () => {
        set({ user: null, isAuthenticated: false, isLoading: false });
      },
      setLoading: (loading) => set({ isLoading: loading }),
    }),
    {
      name: 'mamvibe-auth-mobile',
      storage: createJSONStorage(() => AsyncStorage),
      partialize: (s) => ({ user: s.user }),
      onRehydrateStorage: () => (state) => {
        // Called after async rehydration from AsyncStorage.
        // Always set isLoading = false and derive isAuthenticated from the
        // persisted user so the app never stays stuck in a loading state.
        if (state) {
          state.isAuthenticated = !!state.user;
          state.isLoading = false;
        }
      },
    },
  ),
);
