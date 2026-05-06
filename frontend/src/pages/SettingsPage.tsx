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

type Section = "profile" | "security" | "language" | "payment";

export default function SettingsPage() {
  const { t, i18n } = useTranslation();
  const { user, setUser } = useAuthStore();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [active, setActive] = useState<Section>("profile");

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

  const nav: { id: Section; icon: React.ReactNode; label: string }[] = [
    { id: "profile",  icon: <HiUser className="h-4 w-4" />,       label: t("settings.profile") },
    { id: "security", icon: <HiLockClosed className="h-4 w-4" />, label: t("settings.security") },
    { id: "language", icon: <HiGlobeAlt className="h-4 w-4" />,   label: t("settings.language") },
    { id: "payment",  icon: <HiCreditCard className="h-4 w-4" />, label: t("settings.payment") },
  ];

  const cardBase = "rounded-2xl border border-lavender/20 dark:border-white/10 bg-white dark:bg-[#1e1b2e] shadow-sm";
  const sectionHeader = "px-6 py-3.5 border-b border-lavender/15 dark:border-white/[0.07]";
  const sectionLabel = "text-[11px] font-bold tracking-[0.12em] uppercase text-[#945c67]/70 dark:text-[#c1c4e3]/50";
  const formBody = "px-6 py-5 space-y-4";

  return (
    <div className="max-w-5xl mx-auto px-4 py-10">
      {/* Page header */}
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">
          {t("profile.settings_title")}
        </h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
          {t("settings.subtitle")}
        </p>
      </div>

      <div className="flex gap-7 items-start">
        {/* ── Sidebar (desktop) ── */}
        <aside className="hidden md:flex flex-col w-52 flex-shrink-0 gap-0.5">
          {nav.map(({ id, icon, label }) => (
            <button
              key={id}
              type="button"
              onClick={() => setActive(id)}
              className={`flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm font-medium transition-all text-left w-full ${
                active === id
                  ? "bg-gradient-to-r from-[#945c67] to-[#3f4b7f] text-white shadow-md shadow-[#945c67]/20"
                  : "text-gray-600 dark:text-gray-400 hover:bg-[#945c67]/8 dark:hover:bg-white/5"
              }`}
            >
              <span className={active === id ? "opacity-90" : "text-[#945c67] dark:text-[#c1c4e3]/70"}>
                {icon}
              </span>
              {label}
            </button>
          ))}
        </aside>

        {/* ── Mobile tabs ── */}
        <div className="md:hidden w-full flex gap-2 overflow-x-auto pb-1 mb-4">
          {nav.map(({ id, icon, label }) => (
            <button
              key={id}
              type="button"
              onClick={() => setActive(id)}
              className={`flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-medium whitespace-nowrap flex-shrink-0 transition-all ${
                active === id
                  ? "bg-gradient-to-r from-[#945c67] to-[#3f4b7f] text-white shadow-sm"
                  : "bg-white dark:bg-[#2d2a42] text-gray-600 dark:text-gray-400 border border-lavender/20 dark:border-white/10"
              }`}
            >
              {icon}
              {label}
            </button>
          ))}
        </div>

        {/* ── Content panel ── */}
        <div className="flex-1 min-w-0 space-y-5">

          {/* ════ PROFILE ════ */}
          {active === "profile" && (
            <>
              {/* Identity card */}
              <div className={`${cardBase} overflow-hidden`}>
                <div className="h-28 bg-gradient-to-r from-[#945c67]/50 via-[#3f4b7f]/30 to-transparent dark:from-[#945c67]/30 dark:via-[#3f4b7f]/15" />
                <div className="px-6 pb-6 flex items-end gap-5 -mt-14">
                  <div className="relative flex-shrink-0">
                    <div className="ring-4 ring-white dark:ring-[#1e1b2e] rounded-full">
                      <Avatar src={user?.avatarUrl} profileType={user?.profileType} size="lg" />
                    </div>
                    <button
                      type="button"
                      onClick={() => fileInputRef.current?.click()}
                      disabled={avatarLoading}
                      className="absolute -bottom-1 -right-1 bg-gradient-to-br from-[#945c67] to-[#3f4b7f] text-white rounded-full p-1.5 shadow-md hover:brightness-110 transition disabled:opacity-50"
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
                      className="text-xs font-medium text-[#945c67] hover:underline mt-0.5 disabled:opacity-50"
                    >
                      {avatarLoading ? t("common.loading") : t("profile.change_avatar")}
                    </button>
                  </div>
                </div>
              </div>

              {/* Personal details */}
              <div className={cardBase}>
                <div className={sectionHeader}>
                  <p className={sectionLabel}>{t("settings.personal_details")}</p>
                </div>
                <form onSubmit={handleSubmit} className={formBody}>
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
                      rows={4}
                      placeholder={t("profile.bioPlaceholder")}
                      className="w-full px-4 py-2.5 rounded-xl border border-lavender/40 dark:border-white/10 bg-transparent text-gray-800 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#945c67]/30 focus:border-[#945c67]/50 transition resize-none text-sm"
                    />
                    <p className={`text-xs mt-1 flex justify-between tabular-nums ${form.bio.length >= 480 ? "text-red-500" : "text-gray-400"}`}>
                      <span />
                      <span>{form.bio.length}/500 {t("settings.characters")}</span>
                    </p>
                  </div>
                  <div className="flex justify-end pt-1 border-t border-lavender/10 dark:border-white/5">
                    <Button type="submit" isLoading={loading}>{t("profile.save")}</Button>
                  </div>
                </form>
              </div>
            </>
          )}

          {/* ════ SECURITY ════ */}
          {active === "security" && (
            <div className={cardBase}>
              <div className={sectionHeader}>
                <p className={sectionLabel}>{t("auth.change_password")}</p>
              </div>
              <form onSubmit={handleChangePassword} className={formBody}>
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
                <div className="flex justify-end pt-1 border-t border-lavender/10 dark:border-white/5">
                  <Button type="submit" isLoading={pwLoading}>{t("auth.change_password")}</Button>
                </div>
              </form>
            </div>
          )}

          {/* ════ LANGUAGE ════ */}
          {active === "language" && (
            <div className={cardBase}>
              <div className={sectionHeader}>
                <p className={sectionLabel}>{t("profile.language")}</p>
              </div>
              <div className="px-6 py-5 flex gap-3 flex-wrap">
                {[
                  { code: "en", label: "English", flag: "🇬🇧" },
                  { code: "bg", label: "Български", flag: "🇧🇬" },
                ].map(({ code, label, flag }) => (
                  <button
                    key={code}
                    type="button"
                    onClick={() => handleLanguageChange(code)}
                    className={`flex items-center gap-3 px-5 py-3 rounded-xl border-2 text-sm font-medium transition-all ${
                      i18n.language === code
                        ? "border-[#945c67] bg-gradient-to-r from-[#945c67]/10 to-[#3f4b7f]/8 text-[#945c67] dark:border-[#c1c4e3]/60 dark:text-white shadow-sm"
                        : "border-lavender/30 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:border-[#945c67]/40 hover:bg-[#945c67]/5"
                    }`}
                  >
                    <span className="text-xl leading-none">{flag}</span>
                    <span>{label}</span>
                    {i18n.language === code && (
                      <span className="ml-1 h-2 w-2 rounded-full bg-[#945c67] dark:bg-[#c1c4e3]/80" />
                    )}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* ════ PAYMENT ════ */}
          {active === "payment" && (
            <div className={cardBase}>
              <div className={sectionHeader}>
                <p className={sectionLabel}>{t("payment.iban_label")}</p>
              </div>
              <form onSubmit={handleSubmit} className={formBody}>
                <Input
                  label="IBAN"
                  value={form.iban}
                  onChange={(e) => setForm({ ...form, iban: e.target.value })}
                  placeholder="BG80BNBG96611020345678"
                />
                <p className="text-xs text-gray-400 dark:text-gray-500">
                  {t("settings.iban_hint")}
                </p>
                <div className="flex justify-end pt-1 border-t border-lavender/10 dark:border-white/5">
                  <Button type="submit" isLoading={loading}>{t("profile.save")}</Button>
                </div>
              </form>
            </div>
          )}

        </div>
      </div>
    </div>
  );
}
