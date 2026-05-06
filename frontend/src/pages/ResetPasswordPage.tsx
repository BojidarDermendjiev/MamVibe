import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import { authApi } from '../api/authApi';
import Button from '../components/common/Button';
import Input from '../components/common/Input';

export default function ResetPasswordPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email') || '';
  const token = searchParams.get('token') || '';

  const [form, setForm] = useState({ newPassword: '', confirmNewPassword: '' });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);

  if (!email || !token) {
    return (
      <div className="animate-fade-in text-center">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{t('auth.reset_password_title')}</h1>
        <p className="text-red-500 mt-4">{t('auth.reset_password_invalid')}</p>
      </div>
    );
  }

  const validate = () => {
    const errs: Record<string, string> = {};
    if (form.newPassword.length < 8) {
      errs.newPassword = t('auth.password_min_length');
    } else if (!/[A-Z]/.test(form.newPassword)) {
      errs.newPassword = t('auth.password_uppercase');
    } else if (!/[a-z]/.test(form.newPassword)) {
      errs.newPassword = t('auth.password_lowercase');
    } else if (!/[0-9]/.test(form.newPassword)) {
      errs.newPassword = t('auth.password_digit');
    }
    if (form.newPassword !== form.confirmNewPassword) {
      errs.confirmNewPassword = t('auth.passwords_no_match');
    }
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    setLoading(true);
    try {
      await authApi.resetPassword({ email, token, ...form });
      toast.success(t('auth.reset_password_success'));
      navigate('/login');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="animate-fade-in">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white text-center">{t('auth.reset_password_title')}</h1>
      <p className="text-gray-500 text-center mt-1 mb-6">{email}</p>

      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label={t('auth.new_password')}
          type="password"
          value={form.newPassword}
          onChange={(e) => setForm({ ...form, newPassword: e.target.value })}
          error={errors.newPassword}
          required
        />
        <Input
          label={t('auth.confirm_new_password')}
          type="password"
          value={form.confirmNewPassword}
          onChange={(e) => setForm({ ...form, confirmNewPassword: e.target.value })}
          error={errors.confirmNewPassword}
          required
        />
        <Button type="submit" fullWidth isLoading={loading}>
          {t('auth.reset_password_title')}
        </Button>
      </form>
    </div>
  );
}
