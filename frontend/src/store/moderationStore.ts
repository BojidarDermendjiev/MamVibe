import { create } from 'zustand';
import type { ModerationForbiddenEnvelope, UserModerationStatus } from '../types/moderation';
import { moderationApi } from '../api/moderationApi';

interface ModerationState {
  status: UserModerationStatus | null;
  isLoading: boolean;
  refresh: () => Promise<void>;
  setFromEnvelope: (envelope: ModerationForbiddenEnvelope) => void;
  clear: () => void;
}

/**
 * Holds the current user's moderation snapshot. Hydrated:
 *  - on app init by the auth bootstrap,
 *  - on every 403 with a `moderation` envelope by the axios interceptor,
 *  - by an explicit `refresh()` after the user submits an appeal or admin action lands.
 *
 * The store is the single source of truth the suspension banner reads from.
 */
export const useModerationStore = create<ModerationState>((set) => ({
  status: null,
  isLoading: false,
  refresh: async () => {
    set({ isLoading: true });
    try {
      const { data } = await moderationApi.getMyStatus();
      set({ status: data, isLoading: false });
    } catch {
      // Unauthenticated or transient — leave any prior status in place rather than wiping it.
      set({ isLoading: false });
    }
  },
  setFromEnvelope: (envelope) => {
    set({
      status: {
        level: envelope.moderation.level,
        reason: envelope.moderation.reason,
        publicReason: envelope.moderation.publicReason ?? null,
        startedAt: null,
        expiresAt: envelope.moderation.expiresAt ?? null,
        activeModerationLogId: null,
        canAppeal: envelope.moderation.level !== 'None' && envelope.moderation.level !== 'Warned',
      },
    });
  },
  clear: () => set({ status: null }),
}));
