import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';
import { ProfileType } from '../types/auth';
import Button from '../components/common/Button';
import Input from '../components/common/Input';
import ProfileTypeSelector from '../components/user/ProfileTypeSelector';

export default function RegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const [form, setForm] = useState<{
    email: string;
    password: string;
    confirmPassword: string;
    displayName: string;
    profileType: ProfileType;
  }>({
    email: '',
    password: '',
    confirmPassword: '',
    displayName: '',
    profileType: ProfileType.Female,
  });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const errs: Record<string, string> = {};
    if (form.password.length < 8) {
      errs.password = t('auth.password_min_length');
    } else if (!/[A-Z]/.test(form.password)) {
      errs.password = t('auth.password_uppercase');
    } else if (!/[a-z]/.test(form.password)) {
      errs.password = t('auth.password_lowercase');
    } else if (!/[0-9]/.test(form.password)) {
      errs.password = t('auth.password_digit');
    }
    if (form.password !== form.confirmPassword) errs.confirmPassword = t('auth.passwords_no_match');
    if (!form.displayName.trim()) errs.displayName = t('auth.display_name_required');
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    setLoading(true);
    try {
      const { data } = await authApi.register(form);
      setAuth(data.user, data.accessToken, data.refreshToken);
      toast.success('Account created!');
      navigate('/');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="animate-fade-in">
      <h1 className="text-2xl font-bold text-primary-dark text-center">{t('auth.register_title')}</h1>
      <p className="text-gray-500 text-center mt-1 mb-6">{t('auth.register_subtitle')}</p>

      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label={t('auth.display_name')}
          value={form.displayName}
          onChange={(e) => setForm({ ...form, displayName: e.target.value })}
          error={errors.displayName}
          required
        />
        <Input
          label={t('auth.email')}
          type="email"
          value={form.email}
          onChange={(e) => setForm({ ...form, email: e.target.value })}
          required
        />
        <Input
          label={t('auth.password')}
          type="password"
          value={form.password}
          onChange={(e) => setForm({ ...form, password: e.target.value })}
          error={errors.password}
          required
        />
        <Input
          label={t('auth.confirm_password')}
          type="password"
          value={form.confirmPassword}
          onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
          error={errors.confirmPassword}
          required
        />
        <ProfileTypeSelector
          value={form.profileType}
          onChange={(profileType) => setForm({ ...form, profileType })}
        />
        <Button type="submit" fullWidth isLoading={loading}>
          {t('auth.register_btn')}
        </Button>
      </form>

      <p className="text-center text-sm text-gray-500 mt-6">
        {t('auth.has_account')}{' '}
        <Link to="/login" className="text-primary font-medium hover:underline">
          {t('nav.login')}
        </Link>
      </p>
    </div>
  );
}
