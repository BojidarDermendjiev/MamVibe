import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { Building2, ArrowRight, Loader2, Sun, Moon } from "lucide-react";
import axios from "axios";
import { usePageSEO } from "@/hooks/useSEO";
import { authApi } from "@/api/authApi";
import { useAuthStore } from "@/store/authStore";
import { useTheme } from "@/contexts/ThemeContext";
import LanguageSwitcher from "@/components/common/LanguageSwitcher";
import toast from "@/utils/toast";

/**
 * Dedicated partner (business) registration. Same backend Identity flow as the consumer
 * `/register` page but visually segregated and routes the new user straight into the
 * business profile wizard at `/business/register` once authenticated.
 */
export default function PartnerRegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const { theme, toggleTheme } = useTheme();

  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  usePageSEO({
    title: t("partner.register.seoTitle") || "Create your partner account",
    description: t("partner.register.seoDescription") || "Sign up your coach or venue business on MamVibe.",
    index: false,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setBusy(true);
    setError(null);
    try {
      const { data } = await authApi.register({
        displayName,
        email,
        password,
        confirmPassword: password,
        profileType: 2, // Family (closest neutral choice for a business signup)
      });
      setAuth(data.user, data.accessToken);
      toast.success(t("partner.register.welcome") || "Account created!");
      navigate("/business/register", { replace: true });
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const data = err.response?.data as { error?: string; message?: string } | undefined;
        setError(data?.error || data?.message || t("partner.register.error"));
      } else {
        setError(t("partner.register.error"));
      }
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="relative min-h-screen flex items-center justify-center bg-gradient-to-br from-[#fef6f0] via-[#f5ecff] to-[#fde6db] dark:from-[#1a1825] dark:via-[#26233a] dark:to-[#2d2a42] px-4 py-10 transition-colors duration-300">
      {/* Header chrome — logo links home, language + theme controls top-right.
          Mirrors the consumer auth page so partner visitors get the same
          baseline navigation affordances. */}
      <button
        type="button"
        onClick={() => navigate("/")}
        className="absolute top-5 left-6 z-20 flex items-center gap-2 bg-transparent border-none p-0 cursor-pointer"
      >
        <img src="/logo.png" alt="MamVibe" className="h-9 w-9 object-contain" />
        <span className="text-base font-bold text-[#3f4b7f] dark:text-white">MamVibe</span>
      </button>
      <div className="absolute top-5 right-6 z-20 flex items-center gap-2">
        <LanguageSwitcher />
        <button
          type="button"
          onClick={toggleTheme}
          aria-label={theme === "dark"
            ? t("common.switch_to_light", "Switch to light mode")
            : t("common.switch_to_dark", "Switch to dark mode")}
          className="p-2 rounded-full bg-black/10 dark:bg-white/10 hover:bg-black/20 dark:hover:bg-white/20 transition-all duration-200 text-gray-700 dark:text-gray-200 border border-black/10 dark:border-white/20"
        >
          {theme === "dark" ? <Sun size={16} /> : <Moon size={16} />}
        </button>
      </div>

      <motion.div
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.35 }}
        className="w-full max-w-md"
      >
        <div className="text-center mb-6">
          <div className="inline-flex w-14 h-14 rounded-2xl bg-gradient-to-br from-primary/40 to-mauve/40 items-center justify-center mb-3">
            <Building2 className="h-7 w-7 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-[#3f4b7f] dark:text-white">{t("partner.register.heading")}</h1>
          <p className="text-sm text-gray-600 dark:text-gray-300 mt-1">{t("partner.register.subtitle")}</p>
        </div>

        <form
          onSubmit={handleSubmit}
          className="bg-white dark:bg-white/5 backdrop-blur-md border border-gray-200 dark:border-white/10 rounded-2xl p-6 shadow-xl dark:shadow-2xl space-y-4 transition-colors"
        >
          <Field
            label={t("partner.register.displayNameLabel")}
            value={displayName}
            onChange={setDisplayName}
            placeholder={t("partner.register.displayNamePlaceholder")}
          />
          <Field
            label={t("partner.register.emailLabel")}
            type="email"
            value={email}
            onChange={setEmail}
            placeholder="you@business.com"
          />
          <Field
            label={t("partner.register.passwordLabel")}
            type="password"
            value={password}
            onChange={setPassword}
          />

          <p className="text-[11px] text-gray-500 dark:text-gray-400">{t("partner.register.passwordHint")}</p>

          {error && (
            <p className="text-xs text-red-700 dark:text-red-300 bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-400/20 rounded-lg px-3 py-2">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={busy}
            className="w-full inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 disabled:opacity-60 transition-colors"
          >
            {busy ? (
              <Loader2 size={15} className="animate-spin" />
            ) : (
              <>
                {t("partner.register.signUpButton")} <ArrowRight size={14} />
              </>
            )}
          </button>

          <div className="text-center text-xs text-gray-500 dark:text-gray-400">
            {t("partner.register.haveAccount")}{" "}
            <Link to="/partner/login" className="text-primary hover:underline font-semibold">
              {t("partner.register.signInLink")}
            </Link>
          </div>
        </form>

        <div className="text-center mt-6">
          <Link to="/" className="text-xs text-gray-500 dark:text-gray-400 hover:text-primary dark:hover:text-white transition-colors">
            {t("partner.login.parentLoginLink")}
          </Link>
        </div>
      </motion.div>
    </div>
  );
}

interface FieldProps {
  label: string;
  value: string;
  onChange: (next: string) => void;
  type?: string;
  placeholder?: string;
}
function Field({ label, value, onChange, type = "text", placeholder }: FieldProps) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-700 dark:text-gray-200 mb-1">{label}</label>
      <input
        type={type}
        required
        value={value}
        placeholder={placeholder}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-white/5 text-sm text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-primary/60"
      />
    </div>
  );
}
