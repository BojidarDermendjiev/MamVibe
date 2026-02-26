import { useEffect } from 'react';
import { useAuthStore } from '../store/authStore';
import { authApi } from '../api/authApi';

export function useAuth() {
  const { setAuth, logout, isLoading } = useAuthStore();

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
        if (!cancelled) setAuth(data.user, data.accessToken);
      } catch {
        // No valid cookie — user is not logged in.
        if (!cancelled) logout();
      }
    };

    initAuth();
    return () => { cancelled = true; };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  return { isLoading };
}
