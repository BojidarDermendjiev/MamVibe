import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';
import Button from '../components/common/Button';
import Input from '../components/common/Input';

export default function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const { data } = await authApi.login({ email, password });
      setAuth(data.user, data.accessToken, data.refreshToken);
      toast.success('Welcome back!');
      navigate('/');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleLogin = () => {
    // Google Identity Services
    if (typeof google !== 'undefined') {
      google.accounts.id.initialize({
        client_id: import.meta.env.VITE_GOOGLE_CLIENT_ID || '',
        callback: async (response: { credential: string }) => {
          try {
            const { data } = await authApi.googleLogin({ idToken: response.credential });
            setAuth(data.user, data.accessToken, data.refreshToken);
            toast.success('Welcome!');
            navigate('/');
          } catch {
            toast.error('Google login failed');
          }
        },
      });
      google.accounts.id.prompt();
    }
  };

  return (
    <div className="animate-fade-in">
      <h1 className="text-2xl font-bold text-primary-dark text-center">{t('auth.login_title')}</h1>
      <p className="text-gray-500 text-center mt-1 mb-6">{t('auth.login_subtitle')}</p>

      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label={t('auth.email')}
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <Input
          label={t('auth.password')}
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <div className="text-right">
          <Link to="/forgot-password" className="text-sm text-primary hover:underline">
            {t('auth.forgot_password')}
          </Link>
        </div>
        <Button type="submit" fullWidth isLoading={loading}>
          {t('auth.login_btn')}
        </Button>
      </form>

      <div className="my-6 flex items-center gap-3">
        <div className="flex-1 h-px bg-lavender/50" />
        <span className="text-sm text-gray-400">or</span>
        <div className="flex-1 h-px bg-lavender/50" />
      </div>

      <Button variant="secondary" fullWidth onClick={handleGoogleLogin}>
        {t('auth.google_btn')}
      </Button>

      <p className="text-center text-sm text-gray-500 mt-6">
        {t('auth.no_account')}{' '}
        <Link to="/register" className="text-primary font-medium hover:underline">
          {t('nav.register')}
        </Link>
      </p>
    </div>
  );
}

declare global {
  const google: {
    accounts: {
      id: {
        initialize: (config: { client_id: string; callback: (response: { credential: string }) => void }) => void;
        prompt: () => void;
      };
    };
  };
}
