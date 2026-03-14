import { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import { HiCamera } from 'react-icons/hi';
import axiosClient from '../api/axiosClient';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';
import Avatar from '../components/common/Avatar';
import Button from '../components/common/Button';
import Input from '../components/common/Input';

export default function SettingsPage() {
  const { t, i18n } = useTranslation();
  const { user, setUser } = useAuthStore();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [form, setForm] = useState({
    displayName: user?.displayName || '',
    bio: user?.bio || '',
    iban: user?.iban || '',
  });
  const [loading, setLoading] = useState(false);
  const [avatarLoading, setAvatarLoading] = useState(false);

  const [pwForm, setPwForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: '',
  });
  const [pwErrors, setPwErrors] = useState<Record<string, string>>({});
  const [pwLoading, setPwLoading] = useState(false);

  const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setAvatarLoading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      const { data: uploadData } = await axiosClient.post<{ url: string }>('/photos/upload', formData);

      const { data: updatedUser } = await axiosClient.put('/users/profile', {
        avatarUrl: uploadData.url,
      });
      setUser(updatedUser);
      toast.success(t('profile.avatar_updated'));
    } catch {
      toast.error(t('common.error'));
    } finally {
      setAvatarLoading(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const { data } = await axiosClient.put('/users/profile', form);
      setUser(data);
      toast.success(t('profile.save'));
    } catch {
      toast.error(t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  const validatePassword = () => {
    const errs: Record<string, string> = {};
    if (pwForm.newPassword.length < 8) {
      errs.newPassword = t('auth.password_min_length');
    } else if (!/[A-Z]/.test(pwForm.newPassword)) {
      errs.newPassword = t('auth.password_uppercase');
    } else if (!/[a-z]/.test(pwForm.newPassword)) {
      errs.newPassword = t('auth.password_lowercase');
    } else if (!/[0-9]/.test(pwForm.newPassword)) {
      errs.newPassword = t('auth.password_digit');
    }
    if (pwForm.newPassword !== pwForm.confirmNewPassword) {
      errs.confirmNewPassword = t('auth.passwords_no_match');
    }
    setPwErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validatePassword()) return;
    setPwLoading(true);
    try {
      await authApi.changePassword(pwForm);
      toast.success(t('auth.password_changed'));
      setPwForm({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
      setPwErrors({});
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t('common.error'));
    } finally {
      setPwLoading(false);
    }
  };

  const handleLanguageChange = (lang: string) => {
    i18n.changeLanguage(lang);
    localStorage.setItem('language', lang);
  };

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6">{t('profile.settings_title')}</h1>

      {/* Avatar Section */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-8">
        <label className="block text-sm font-medium text-primary mb-3">{t('profile.avatar')}</label>
        <div className="flex items-center gap-5">
          <div className="relative">
            <Avatar
              src={user?.avatarUrl}
              profileType={user?.profileType}
              size="lg"
            />
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={avatarLoading}
              className="absolute -bottom-1 -right-1 bg-primary text-white rounded-full p-1.5 shadow-md hover:bg-primary-dark transition-colors disabled:opacity-50"
            >
              <HiCamera className="h-4 w-4" />
            </button>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              onChange={handleAvatarChange}
              className="hidden"
            />
          </div>
          <div>
            <p className="text-sm font-medium text-gray-800">{user?.displayName}</p>
            <p className="text-xs text-gray-500">{user?.email}</p>
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={avatarLoading}
              className="text-sm text-primary hover:underline mt-1 disabled:opacity-50"
            >
              {avatarLoading ? t('common.loading') : t('profile.change_avatar')}
            </button>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl p-6 border border-lavender/30 space-y-5">
        <Input
          label={t('auth.display_name')}
          value={form.displayName}
          onChange={(e) => setForm({ ...form, displayName: e.target.value })}
          required
        />

        <div>
          <label className="block text-sm font-medium text-primary mb-1">{t('profile.bio')}</label>
          <textarea
            value={form.bio}
            onChange={(e) => setForm({ ...form, bio: e.target.value })}
            rows={3}
            className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white text-gray-800 focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
          />
        </div>

        <Input
          label={t('payment.iban_label')}
          value={form.iban}
          onChange={(e) => setForm({ ...form, iban: e.target.value })}
          placeholder="BG80BNBG96611020345678"
        />

        <div>
          <label className="block text-sm font-medium text-primary mb-1">{t('profile.language')}</label>
          <div className="flex gap-3">
            <button
              type="button"
              onClick={() => handleLanguageChange('en')}
              className={`px-4 py-2 rounded-lg border-2 text-sm font-medium transition-colors ${
                i18n.language === 'en' ? 'border-primary bg-primary/10 text-primary' : 'border-gray-200 text-gray-500'
              }`}
            >
              English
            </button>
            <button
              type="button"
              onClick={() => handleLanguageChange('bg')}
              className={`px-4 py-2 rounded-lg border-2 text-sm font-medium transition-colors ${
                i18n.language === 'bg' ? 'border-primary bg-primary/10 text-primary' : 'border-gray-200 text-gray-500'
              }`}
            >
              Български
            </button>
          </div>
        </div>

        <Button type="submit" fullWidth isLoading={loading}>
          {t('profile.save')}
        </Button>
      </form>

      <form onSubmit={handleChangePassword} className="bg-white rounded-xl p-6 border border-lavender/30 space-y-5 mt-8">
        <h2 className="text-xl font-bold text-primary">{t('auth.change_password')}</h2>

        <Input
          label={t('auth.current_password')}
          type="password"
          value={pwForm.currentPassword}
          onChange={(e) => setPwForm({ ...pwForm, currentPassword: e.target.value })}
          required
        />
        <Input
          label={t('auth.new_password')}
          type="password"
          value={pwForm.newPassword}
          onChange={(e) => setPwForm({ ...pwForm, newPassword: e.target.value })}
          error={pwErrors.newPassword}
          required
        />
        <Input
          label={t('auth.confirm_new_password')}
          type="password"
          value={pwForm.confirmNewPassword}
          onChange={(e) => setPwForm({ ...pwForm, confirmNewPassword: e.target.value })}
          error={pwErrors.confirmNewPassword}
          required
        />

        <Button type="submit" fullWidth isLoading={pwLoading}>
          {t('auth.change_password')}
        </Button>
      </form>
    </div>
  );
}
