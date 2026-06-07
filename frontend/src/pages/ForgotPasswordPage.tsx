import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Mail } from 'lucide-react';
import { authApi } from '../api/authApi';
import { usePageSEO } from '@/hooks/useSEO';

export default function ForgotPasswordPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  usePageSEO({ title: "Reset Your Password", description: "Reset your MamVibe account password.", index: false });

  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await authApi.forgotPassword({ email });
    } catch {
      // Always show success to prevent email enumeration
    } finally {
      setLoading(false);
      setSent(true);
    }
  };

  if (sent) {
    return (
      <div className="text-center">
        <h1 className="auth-title mb-3">{t('auth.forgot_password_title')}</h1>
        <p style={{ color: '#6b6888', fontSize: '0.85rem', marginBottom: '1.5rem' }}>{t('auth.forgot_password_sent')}</p>
        <button type="button" onClick={() => navigate('/login')} className="auth-btn-fill">
          {t('nav.login')}
        </button>
      </div>
    );
  }

  return (
    <div>
      <h1 className="auth-title mb-1">{t('auth.forgot_password_title')}</h1>
      <p style={{ color: '#6b6888', fontSize: '0.8rem', textAlign: 'center', marginBottom: '1.25rem' }}>
        {t('auth.forgot_password_desc')}
      </p>

      <form onSubmit={handleSubmit}>
        <div className="auth-fields" style={{ marginBottom: '1rem' }}>
          <div className="auth-field">
            <Mail className="auth-field-icon" size={16} />
            <input
              type="email"
              placeholder={t('auth.email')}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="auth-field-input"
              required
              autoComplete="email"
            />
          </div>
        </div>

        <button type="submit" disabled={loading} className="auth-btn-fill">
          {loading ? t('common.loading') : t('auth.send_reset_link')}
        </button>
      </form>
    </div>
  );
}
