import { create } from "zustand";
import type { BusinessProfileDto } from "../types/business";

interface BusinessState {
  profile: BusinessProfileDto | null;
  isLoading: boolean;
  isLoaded: boolean;
  setProfile: (profile: BusinessProfileDto | null) => void;
  setLoading: (loading: boolean) => void;
  reset: () => void;
}

/**
 * Lightweight cache of the currently signed-in user's BusinessProfile. Populated by
 * the dashboard and registration pages; consumers should treat `isLoaded=false` as
 * "haven't checked yet" and `profile=null && isLoaded=true` as "user has no profile".
 */
export const useBusinessStore = create<BusinessState>((set) => ({
  profile: null,
  isLoading: false,
  isLoaded: false,
  setProfile: (profile) =>
    set({ profile, isLoading: false, isLoaded: true }),
  setLoading: (isLoading) => set({ isLoading }),
  reset: () => set({ profile: null, isLoading: false, isLoaded: false }),
}));
