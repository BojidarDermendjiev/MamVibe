import { useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { authApi } from '../api/authApi';
import Button from '../components/common/Button';
import Input from '../components/common/Input';
import TurnstileWidget from '../components/common/TurnstileWidget';
import { usePageSEO } from '@/hooks/useSEO';

export default function ForgotPasswordPage() {
  const { t } = useTranslation();

  usePageSEO({ title: "Reset Your Password", description: "Reset your MamVibe account password.", index: false });
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);
  const [turnstileToken, setTurnstileToken] = useState<string | null>(null);
  const clearToken = useCallback(() => setTurnstileToken(null), []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await authApi.forgotPassword({ email, turnstileToken: turnstileToken ?? undefined });
    } catch {
      // Always show success to prevent email enumeration
    } finally {
      setLoading(false);
      setSent(true);
    }
  };

  return (
    <div className="animate-fade-in">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white text-center">{t('auth.forgot_password_title')}</h1>
      <p className="text-gray-500 text-center mt-1 mb-6">{t('auth.forgot_password_desc')}</p>

      {sent ? (
        <div className="text-center space-y-4">
          <p className="text-green-600 font-medium">{t('auth.forgot_password_sent')}</p>
          <Link to="/login" className="text-primary font-medium hover:underline">
            {t('nav.login')}
          </Link>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label={t('auth.email')}
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          <div className="flex justify-center">
            <TurnstileWidget onToken={setTurnstileToken} onExpire={clearToken} />
          </div>
          <Button type="submit" fullWidth isLoading={loading} disabled={!turnstileToken}>
            {t('auth.send_reset_link')}
          </Button>
        </form>
      )}

      <p className="text-center text-sm text-gray-500 mt-6">
        <Link to="/login" className="text-primary font-medium hover:underline">
          {t('nav.login')}
        </Link>
      </p>
    </div>
  );
}
