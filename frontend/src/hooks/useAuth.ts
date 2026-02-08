import { useEffect } from 'react';
import { useAuthStore } from '../store/authStore';
import { authApi } from '../api/authApi';

export function useAuth() {
  const { setUser, setLoading, accessToken, logout, isLoading } = useAuthStore();

  useEffect(() => {
    const initAuth = async () => {
      if (!accessToken) {
        setLoading(false);
        return;
      }
      try {
        const { data } = await authApi.me();
        setUser(data);
      } catch {
        logout();
      } finally {
        setLoading(false);
      }
    };
    initAuth();
  }, []);

  return { isLoading };
}
