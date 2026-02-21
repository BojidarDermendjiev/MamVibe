import { useState, useEffect, useRef, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Mail, Lock, Eye, EyeOff } from 'lucide-react';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';

interface GoogleAccountsId {
  initialize: (config: {
    client_id: string;
    callback: (response: { credential: string }) => void;
    use_fedcm_for_prompt?: boolean;
  }) => void;
  renderButton: (
    parent: HTMLElement,
    options: {
      type?: 'standard' | 'icon';
      theme?: 'outline' | 'filled_blue' | 'filled_black';
      size?: 'large' | 'medium' | 'small';
      shape?: 'rectangular' | 'pill' | 'circle' | 'square';
      width?: number;
      text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin';
      logo_alignment?: 'left' | 'center';
    },
  ) => void;
  cancel: () => void;
}

declare global {
  // var (not const) so TypeScript allows `typeof google === 'undefined'` narrowing
  var google: { accounts: { id: GoogleAccountsId } } | undefined;
}

export default function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [googleLoading, setGoogleLoading] = useState(false);
  const [googleReady, setGoogleReady] = useState(false);

  const googleButtonRef = useRef<HTMLDivElement>(null);
  const googleCredentialHandlerRef = useRef<((credential: string) => void) | null>(null);

  googleCredentialHandlerRef.current = useCallback(
    async (credential: string) => {
      setGoogleLoading(true);
      try {
        const { data } = await authApi.googleLogin({ idToken: credential });
        setAuth(data.user, data.accessToken, data.refreshToken);
        toast.success('Welcome!');
        navigate('/');
      } catch (err: unknown) {
        const msg =
          (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
        toast.error(msg || 'Google login failed. Please try again.');
      } finally {
        setGoogleLoading(false);
      }
    },
    [setAuth, navigate],
  );

  // ── Google Identity Services ──
  // Waits for the GSI script (loaded in index.html) to be ready,
  // then renders the official Google Sign-In button.
  useEffect(() => {
    const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
    if (!clientId) return;

    const renderGoogleButton = () => {
      if (typeof google === 'undefined' || !googleButtonRef.current) return;

      google.accounts.id.initialize({
        client_id: clientId,
        callback: (response) => googleCredentialHandlerRef.current?.(response.credential),
        use_fedcm_for_prompt: false,
      });

      const containerWidth = googleButtonRef.current.offsetWidth || 320;
      google.accounts.id.renderButton(googleButtonRef.current, {
        type: 'standard',
        size: 'large',
        theme: 'outline',
        shape: 'rectangular',
        width: containerWidth,
        text: 'signin_with',
        logo_alignment: 'center',
      });

      setGoogleReady(true);
    };

    if (typeof google !== 'undefined') {
      renderGoogleButton();
    } else {
      const interval = setInterval(() => {
        if (typeof google !== 'undefined') {
          clearInterval(interval);
          renderGoogleButton();
        }
      }, 100);
      return () => {
        clearInterval(interval);
        if (typeof google !== 'undefined') google.accounts.id.cancel();
      };
    }

    return () => {
      if (typeof google !== 'undefined') google.accounts.id.cancel();
    };
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const { data } = await authApi.login({ email, password });
      setAuth(data.user, data.accessToken, data.refreshToken);

      toast.success('Welcome back!');
      navigate('/');
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 text-center mb-6">Sign in</h1>

      {/* autoComplete="on" + name/id/autocomplete attributes tell Chrome
          to save and restore credentials via Windows Hello automatically */}
      <form onSubmit={handleSubmit} className="space-y-3" autoComplete="on">
        {/* Email */}
        <div className="relative">
          <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          <input
            type="email"
            id="email"
            name="email"
            autoComplete="username"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder={t('auth.email')}
            required
            className="w-full pl-11 pr-4 py-3 rounded-full bg-gray-100 border-none text-gray-700 placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-colors"
          />
        </div>

        {/* Password */}
        <div className="relative">
          <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          <input
            type={showPassword ? 'text' : 'password'}
            id="password"
            name="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder={t('auth.password')}
            required
            className="w-full pl-11 pr-11 py-3 rounded-full bg-gray-100 border-none text-gray-700 placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-colors"
          />
          <button
            type="button"
            onClick={() => setShowPassword((v) => !v)}
            className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
            tabIndex={-1}
          >
            {showPassword ? <EyeOff size={15} /> : <Eye size={15} />}
          </button>
        </div>

        {/* Forgot */}
        <div className="flex justify-end pr-1">
          <Link
            to="/forgot-password"
            className="text-xs text-gray-400 hover:text-primary transition-colors"
          >
            {t('auth.forgot_password')}
          </Link>
        </div>

        {/* Submit */}
        <button
          type="submit"
          disabled={loading}
          className="w-full py-3 rounded-full font-bold text-sm text-white uppercase tracking-widest transition-all duration-300 hover:opacity-90 hover:shadow-lg hover:shadow-primary/30 disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ background: 'linear-gradient(135deg, #945c67 0%, #3f4b7f 100%)' }}
        >
          {loading ? 'Signing in…' : 'Login'}
        </button>
      </form>

      {/* Google Sign-In */}
      <div className="mt-5">
        <div className="relative flex items-center gap-3 mb-4">
          <div className="flex-1 h-px bg-gray-200" />
          <span className="text-xs text-gray-400 shrink-0">or continue with</span>
          <div className="flex-1 h-px bg-gray-200" />
        </div>

        <div className="flex justify-center">
          {googleLoading && (
            <div className="flex items-center gap-2 text-sm text-gray-500 py-2">
              <span className="w-4 h-4 border-2 border-[#EA4335] border-t-transparent rounded-full animate-spin" />
              Signing in with Google…
            </div>
          )}
          <div
            ref={googleButtonRef}
            className={`w-full ${googleLoading ? 'hidden' : ''}`}
            style={{ minHeight: googleReady ? undefined : '44px' }}
          />
          {!googleReady && !googleLoading && (
            <div className="w-full h-11 rounded-md bg-gray-100 animate-pulse" />
          )}
        </div>
      </div>
    </div>
  );
}
