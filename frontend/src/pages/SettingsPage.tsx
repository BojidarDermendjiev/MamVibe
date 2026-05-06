import { useState, useRef } from "react";
import { useTranslation } from "react-i18next";
import toast from "@/utils/toast";
import { HiCamera, HiUser, HiLockClosed, HiGlobeAlt, HiCreditCard } from "react-icons/hi2";
import axiosClient from "../api/axiosClient";
import { authApi } from "../api/authApi";
import { useAuthStore } from "../store/authStore";
import Avatar from "../components/common/Avatar";
import Button from "../components/common/Button";
import Input from "../components/common/Input";

function SectionCard({ icon, title, children }: { icon: React.ReactNode; title: string; children: React.ReactNode }) {
  return (
    <div className="rounded-2xl border border-lavender/20 dark:border-white/10 bg-white dark:bg-[#1e1b2e] shadow-sm overflow-hidden">
      <div className="flex items-center gap-2.5 px-6 py-4 border-b border-lavender/20 dark:border-white/10">
        <span className="text-primary">{icon}</span>
        <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-200 tracking-wide uppercase">{title}</h2>
      </div>
      <div className="px-6 py-5">{children}</div>
    </div>
  );
}

export default function SettingsPage() {
  const { t, i18n } = useTranslation();
  const { user, setUser } = useAuthStore();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [form, setForm] = useState({
    displayName: user?.displayName || "",
    bio: user?.bio || "",
    iban: user?.iban || "",
  });
  const [loading, setLoading] = useState(false);
  const [avatarLoading, setAvatarLoading] = useState(false);

  const [pwForm, setPwForm] = useState({
    currentPassword: "",
    newPassword: "",
    confirmNewPassword: "",
  });
  const [pwErrors, setPwErrors] = useState<Record<string, string>>({});
  const [pwLoading, setPwLoading] = useState(false);

  const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setAvatarLoading(true);
    try {
      const formData = new FormData();
      formData.append("file", file);
      const { data: uploadData } = await axiosClient.post<{ url: string }>("/photos/upload", formData);
      const { data: updatedUser } = await axiosClient.put("/users/profile", { avatarUrl: uploadData.url });
      setUser(updatedUser);
      toast.success(t("profile.avatar_updated"));
    } catch {
      toast.error(t("common.error"));
    } finally {
      setAvatarLoading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const { data } = await axiosClient.put("/users/profile", form);
      setUser(data);
      toast.success(t("profile.save"));
    } catch {
      toast.error(t("common.error"));
    } finally {
      setLoading(false);
    }
  };

  const validatePassword = () => {
    const errs: Record<string, string> = {};
    if (pwForm.newPassword.length < 8) errs.newPassword = t("auth.password_min_length");
    else if (!/[A-Z]/.test(pwForm.newPassword)) errs.newPassword = t("auth.password_uppercase");
    else if (!/[a-z]/.test(pwForm.newPassword)) errs.newPassword = t("auth.password_lowercase");
    else if (!/[0-9]/.test(pwForm.newPassword)) errs.newPassword = t("auth.password_digit");
    if (pwForm.newPassword !== pwForm.confirmNewPassword) errs.confirmNewPassword = t("auth.passwords_no_match");
    setPwErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validatePassword()) return;
    setPwLoading(true);
    try {
      await authApi.changePassword(pwForm);
      toast.success(t("auth.password_changed"));
      setPwForm({ currentPassword: "", newPassword: "", confirmNewPassword: "" });
      setPwErrors({});
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t("common.error"));
    } finally {
      setPwLoading(false);
    }
  };

  const handleLanguageChange = (lang: string) => {
    i18n.changeLanguage(lang);
    localStorage.setItem("language", lang);
  };

  return (
    <div className="max-w-2xl mx-auto px-4 py-10 space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">
        {t("profile.settings_title")}
      </h1>

      {/* ── Profile Identity Card ── */}
      <div className="rounded-2xl border border-lavender/20 dark:border-white/10 bg-white dark:bg-[#1e1b2e] shadow-sm overflow-hidden">
        {/* Banner */}
        <div className="h-24 bg-gradient-to-r from-primary/40 via-primary/20 to-transparent dark:from-primary/30 dark:via-primary/10" />
        {/* Avatar + meta */}
        <div className="px-6 pb-6 flex items-end gap-5 -mt-12">
          <div className="relative flex-shrink-0">
            <div className="ring-4 ring-white dark:ring-[#1e1b2e] rounded-full">
              <Avatar src={user?.avatarUrl} profileType={user?.profileType} size="lg" />
            </div>
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={avatarLoading}
              className="absolute -bottom-1 -right-1 bg-primary text-white rounded-full p-1.5 shadow-md hover:brightness-110 transition disabled:opacity-50"
              title={t("profile.change_avatar")}
            >
              <HiCamera className="h-3.5 w-3.5" />
            </button>
            <input ref={fileInputRef} type="file" accept="image/*" onChange={handleAvatarChange} className="hidden" />
          </div>
          <div className="pb-1 min-w-0">
            <p className="font-semibold text-gray-900 dark:text-white truncate">{user?.displayName}</p>
            <p className="text-sm text-gray-500 dark:text-gray-400 truncate">{user?.email}</p>
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={avatarLoading}
              className="text-xs text-primary hover:underline mt-0.5 disabled:opacity-50"
            >
              {avatarLoading ? t("common.loading") : t("profile.change_avatar")}
            </button>
          </div>
        </div>
      </div>

      {/* ── Profile Information ── */}
      <SectionCard icon={<HiUser className="h-4 w-4" />} title={t("settings.profile")}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label={t("auth.display_name")}
            value={form.displayName}
            onChange={(e) => setForm({ ...form, displayName: e.target.value })}
            required
          />

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t("profile.bio")}
            </label>
            <textarea
              value={form.bio}
              onChange={(e) => setForm({ ...form, bio: e.target.value })}
              maxLength={500}
              rows={3}
              placeholder={t("profile.bioPlaceholder") ?? "Tell families about yourself…"}
              className="w-full px-4 py-2.5 rounded-xl border border-lavender/40 dark:border-white/10 bg-transparent text-gray-800 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition resize-none text-sm"
            />
            <p className={`text-xs mt-1 text-right tabular-nums ${form.bio.length >= 480 ? "text-red-500" : "text-gray-400"}`}>
              {form.bio.length}/500
            </p>
          </div>

          <Button type="submit" fullWidth isLoading={loading}>
            {t("profile.save")}
          </Button>
        </form>
      </SectionCard>

      {/* ── Payment Details ── */}
      <SectionCard icon={<HiCreditCard className="h-4 w-4" />} title={t("payment.iban_label") ?? "Payment Details"}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label="IBAN"
            value={form.iban}
            onChange={(e) => setForm({ ...form, iban: e.target.value })}
            placeholder="BG80BNBG96611020345678"
          />
          <p className="text-xs text-gray-400 dark:text-gray-500 -mt-1">
            {t("payment.iban_hint") ?? "Buyers will use this to transfer funds for on-spot payments."}
          </p>
          <Button type="submit" fullWidth isLoading={loading}>
            {t("profile.save")}
          </Button>
        </form>
      </SectionCard>

      {/* ── Language ── */}
      <SectionCard icon={<HiGlobeAlt className="h-4 w-4" />} title={t("profile.language") ?? "Language"}>
        <div className="flex gap-3">
          {[
            { code: "en", label: "English", flag: "🇬🇧" },
            { code: "bg", label: "Български", flag: "🇧🇬" },
          ].map(({ code, label, flag }) => (
            <button
              key={code}
              type="button"
              onClick={() => handleLanguageChange(code)}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-xl border-2 text-sm font-medium transition-all ${
                i18n.language === code
                  ? "border-primary bg-primary/10 text-primary dark:bg-primary/20"
                  : "border-lavender/30 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:border-primary/40"
              }`}
            >
              <span className="text-base">{flag}</span>
              {label}
              {i18n.language === code && <span className="ml-1 h-1.5 w-1.5 rounded-full bg-primary" />}
            </button>
          ))}
        </div>
      </SectionCard>

      {/* ── Security ── */}
      <SectionCard icon={<HiLockClosed className="h-4 w-4" />} title={t("auth.change_password") ?? "Security"}>
        <form onSubmit={handleChangePassword} className="space-y-4">
          <Input
            label={t("auth.current_password")}
            type="password"
            value={pwForm.currentPassword}
            onChange={(e) => setPwForm({ ...pwForm, currentPassword: e.target.value })}
            required
          />
          <Input
            label={t("auth.new_password")}
            type="password"
            value={pwForm.newPassword}
            onChange={(e) => setPwForm({ ...pwForm, newPassword: e.target.value })}
            error={pwErrors.newPassword}
            required
          />
          <Input
            label={t("auth.confirm_new_password")}
            type="password"
            value={pwForm.confirmNewPassword}
            onChange={(e) => setPwForm({ ...pwForm, confirmNewPassword: e.target.value })}
            error={pwErrors.confirmNewPassword}
            required
          />
          <Button type="submit" fullWidth isLoading={pwLoading}>
            {t("auth.change_password")}
          </Button>
        </form>
      </SectionCard>
    </div>
  );
}
