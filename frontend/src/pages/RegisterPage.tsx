import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { User, Mail, Lock, Eye, EyeOff } from 'lucide-react';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';
import { ProfileType } from '../types/auth';
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
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const validate = () => {
    const errs: Record<string, string> = {};
    if (form.password.length < 8) errs.password = t('auth.password_min_length');
    else if (!/[A-Z]/.test(form.password)) errs.password = t('auth.password_uppercase');
    else if (!/[a-z]/.test(form.password)) errs.password = t('auth.password_lowercase');
    else if (!/[0-9]/.test(form.password)) errs.password = t('auth.password_digit');
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
    <div>
      <h1 className="text-2xl font-bold text-gray-800 text-center mb-5">Sign up</h1>

      <form onSubmit={handleSubmit} className="space-y-2.5">
        {/* Display name */}
        <div>
          <div className="relative">
            <User className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
            <input
              value={form.displayName}
              onChange={(e) => setForm({ ...form, displayName: e.target.value })}
              placeholder={t('auth.display_name')}
              required
              className="w-full pl-11 pr-4 py-3 rounded-full bg-gray-100 border-none text-gray-700 placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-colors"
            />
          </div>
          {errors.displayName && <p className="mt-1 text-xs text-red-500 pl-4">{errors.displayName}</p>}
        </div>

        {/* Email */}
        <div className="relative">
          <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          <input
            type="email"
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
            placeholder={t('auth.email')}
            required
            className="w-full pl-11 pr-4 py-3 rounded-full bg-gray-100 border-none text-gray-700 placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-colors"
          />
        </div>

        {/* Password */}
        <div>
          <div className="relative">
            <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
            <input
              type={showPassword ? 'text' : 'password'}
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
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
          {errors.password && <p className="mt-1 text-xs text-red-500 pl-4">{errors.password}</p>}
        </div>

        {/* Confirm password */}
        <div>
          <div className="relative">
            <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
            <input
              type={showConfirm ? 'text' : 'password'}
              value={form.confirmPassword}
              onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
              placeholder={t('auth.confirm_password')}
              required
              className="w-full pl-11 pr-11 py-3 rounded-full bg-gray-100 border-none text-gray-700 placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-colors"
            />
            <button
              type="button"
              onClick={() => setShowConfirm((v) => !v)}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
              tabIndex={-1}
            >
              {showConfirm ? <EyeOff size={15} /> : <Eye size={15} />}
            </button>
          </div>
          {errors.confirmPassword && <p className="mt-1 text-xs text-red-500 pl-4">{errors.confirmPassword}</p>}
        </div>

        {/* Profile type */}
        <ProfileTypeSelector
          value={form.profileType}
          onChange={(profileType) => setForm({ ...form, profileType })}
        />

        {/* Submit */}
        <button
          type="submit"
          disabled={loading}
          className="w-full py-3 rounded-full font-bold text-sm text-white uppercase tracking-widest transition-all duration-300 hover:opacity-90 hover:shadow-lg hover:shadow-primary/30 disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ background: 'linear-gradient(135deg, #945c67 0%, #3f4b7f 100%)' }}
        >
          {loading ? 'Creating account…' : 'Sign Up'}
        </button>
      </form>

    </div>
  );
}
