import { useState, useEffect, useRef, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Mail, Lock, Eye, EyeOff } from 'lucide-react';
import { FaGoogle, FaFacebook, FaTwitter, FaLinkedinIn } from 'react-icons/fa';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';

declare global {
  const google: {
    accounts: {
      id: {
        initialize: (config: {
          client_id: string;
          callback: (response: { credential: string }) => void;
          use_fedcm_for_prompt?: boolean;
        }) => void;
        prompt: (momentListener?: (notification: {
          isNotDisplayed: () => boolean;
          isSkippedMoment: () => boolean;
          getMomentType: () => string;
        }) => void) => void;
        renderButton: (
          parent: HTMLElement,
          options: {
            type?: 'standard' | 'icon';
            theme?: 'outline' | 'filled_blue' | 'filled_black';
            size?: 'large' | 'medium' | 'small';
            shape?: 'rectangular' | 'pill' | 'circle' | 'square';
          },
        ) => void;
        cancel: () => void;
      };
    };
  };
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

  // Fallback container for Google's renderButton (hidden, used to trigger clicks when prompt() is suppressed)
  const googleFallbackRef = useRef<HTMLDivElement>(null);
  // Stable ref so the GSI callback always sees the latest navigate/setAuth without re-initializing
  const credentialHandlerRef = useRef<((credential: string) => void) | null>(null);

  credentialHandlerRef.current = useCallback(
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

  // Initialize Google Identity Services once on mount
  useEffect(() => {
    const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
    if (!clientId || typeof google === 'undefined') return;

    google.accounts.id.initialize({
      client_id: clientId,
      // Route through the stable ref so re-renders never cause stale closures
      callback: (response) => credentialHandlerRef.current?.(response.credential),
      use_fedcm_for_prompt: false, // use legacy One Tap, not FedCM (more compatible)
    });

    // Render a real Google button into the hidden container as fallback
    if (googleFallbackRef.current) {
      google.accounts.id.renderButton(googleFallbackRef.current, {
        type: 'icon',
        size: 'large',
        shape: 'circle',
        theme: 'outline',
      });
    }

    return () => {
      if (typeof google !== 'undefined') {
        google.accounts.id.cancel();
      }
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const handleGoogleLogin = () => {
    if (typeof google === 'undefined') {
      toast.error('Google Sign-In is not available. Please refresh the page.');
      return;
    }

    google.accounts.id.prompt((notification) => {
      if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
        // One Tap was suppressed (cookie block, prior dismissal, FedCM, etc.)
        // Fall back to clicking the actual Google-rendered button
        const btn = googleFallbackRef.current?.querySelector(
          'div[role="button"]',
        ) as HTMLElement | null;
        if (btn) {
          btn.click();
        } else {
          toast.error(
            'Google Sign-In could not be shown. Please allow pop-ups or try again.',
          );
        }
      }
    });
  };

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
      {/* Hidden Google renderButton container — fallback when prompt() is suppressed */}
      <div ref={googleFallbackRef} className="hidden" aria-hidden="true" />

      <h1 className="text-2xl font-bold text-gray-800 text-center mb-6">Sign in</h1>

      <form onSubmit={handleSubmit} className="space-y-3">
        {/* Email */}
        <div className="relative">
          <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          <input
            type="email"
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
        <div className="text-right pr-1">
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

      {/* Social */}
      <div className="mt-6 text-center">
        <p className="text-xs text-gray-400 mb-4">Or sign in with social platforms</p>
        <div className="flex items-center justify-center gap-3">
          <button
            type="button"
            onClick={handleGoogleLogin}
            disabled={googleLoading}
            title="Sign in with Google"
            className="w-9 h-9 rounded-full border border-gray-200 flex items-center justify-center hover:border-primary hover:bg-gray-50 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {googleLoading ? (
              <span className="w-4 h-4 border-2 border-[#EA4335] border-t-transparent rounded-full animate-spin" />
            ) : (
              <FaGoogle className="w-4 h-4 text-[#EA4335]" />
            )}
          </button>
          <button
            type="button"
            disabled
            title="Facebook (coming soon)"
            className="w-9 h-9 rounded-full border border-gray-200 flex items-center justify-center opacity-50 cursor-not-allowed"
          >
            <FaFacebook className="w-4 h-4 text-[#1877F2]" />
          </button>
          <button
            type="button"
            disabled
            title="Twitter (coming soon)"
            className="w-9 h-9 rounded-full border border-gray-200 flex items-center justify-center opacity-50 cursor-not-allowed"
          >
            <FaTwitter className="w-4 h-4 text-[#1DA1F2]" />
          </button>
          <button
            type="button"
            disabled
            title="LinkedIn (coming soon)"
            className="w-9 h-9 rounded-full border border-gray-200 flex items-center justify-center opacity-50 cursor-not-allowed"
          >
            <FaLinkedinIn className="w-4 h-4 text-[#0A66C2]" />
          </button>
        </div>
      </div>
    </div>
  );
}
