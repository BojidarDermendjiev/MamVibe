import { useEffect } from 'react';
import { useAuthStore } from '../store/authStore';
import { useModerationStore } from '../store/moderationStore';
import { authApi } from '../api/authApi';

export function useAuth() {
  const { setAuth, logout, isLoading } = useAuthStore();
  const refreshModeration = useModerationStore((s) => s.refresh);
  const clearModeration = useModerationStore((s) => s.clear);

  useEffect(() => {
    // The `cancelled` flag guards against stale promise resolution after unmount
    // or rapid remount. If the component unmounts before the refresh resolves,
    // the flag is set to true and the state update is skipped — preventing
    // "state update on unmounted component" warnings and stale auth state.
    let cancelled = false;

    const initAuth = async () => {
      try {
        // Attempt a silent token refresh using the httpOnly cookie.
        // If the cookie is valid the backend returns a fresh access token.
        const { data } = await authApi.refresh();
        if (!cancelled) {
          setAuth(data.user, data.accessToken);
          // Hydrate the moderation snapshot so the suspension banner renders correctly
          // before the user touches a write endpoint.
          refreshModeration();
        }
      } catch {
        // No valid cookie — user is not logged in.
        if (!cancelled) {
          logout();
          clearModeration();
        }
      }
    };

    initAuth();
    return () => { cancelled = true; };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  return { isLoading };
}
