import { useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { Building2, ArrowRight, Loader2, LogIn } from "lucide-react";
import axios from "axios";
import { usePageSEO } from "@/hooks/useSEO";
import { authApi } from "@/api/authApi";
import { useAuthStore } from "@/store/authStore";
import toast from "@/utils/toast";

/**
 * Dedicated partner (business) login page. Hits the same `/auth/login` endpoint as the
 * consumer login, but visually segregates the entry point and routes successful logins
 * to `/business/dashboard` instead of the home page — addressing the "businesses need
 * their own login" requirement without splitting the underlying Identity backend.
 */
export default function PartnerLoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { setAuth } = useAuthStore();

  const nextPath = searchParams.get("next") || "/business/dashboard";

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  usePageSEO({
    title: t("partner.login.seoTitle") || "Partner sign in",
    description: t("partner.login.seoDescription") || "Sign in to manage your business listing on MamVibe.",
    index: false,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setBusy(true);
    setError(null);
    try {
      const { data } = await authApi.login({ email, password });
      setAuth(data.user, data.accessToken);
      toast.success(
        t("partner.login.welcome", { name: data.user.displayName }) || "Welcome back!",
      );
      navigate(nextPath, { replace: true });
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const message = (err.response?.data as { error?: string; message?: string } | undefined);
        setError(message?.error || message?.message || t("partner.login.invalidCredentials"));
      } else {
        setError(t("partner.login.invalidCredentials"));
      }
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-[#1a1825] via-[#26233a] to-[#2d2a42] px-4 py-10">
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
          <h1 className="text-2xl font-bold text-white">{t("partner.login.heading")}</h1>
          <p className="text-sm text-gray-300 mt-1">{t("partner.login.subtitle")}</p>
        </div>

        <form
          onSubmit={handleSubmit}
          className="bg-white/5 backdrop-blur-md border border-white/10 rounded-2xl p-6 shadow-2xl space-y-4"
        >
          <div>
            <label className="block text-xs font-medium text-gray-200 mb-1">
              {t("partner.login.emailLabel")}
            </label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@business.com"
              className="w-full px-3 py-2 rounded-lg border border-white/10 bg-white/5 text-sm text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-primary/60"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-200 mb-1">
              {t("partner.login.passwordLabel")}
            </label>
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-3 py-2 rounded-lg border border-white/10 bg-white/5 text-sm text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-primary/60"
            />
          </div>

          {error && (
            <p className="text-xs text-red-300 bg-red-500/10 border border-red-400/20 rounded-lg px-3 py-2">
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
                <LogIn size={15} />
                {t("partner.login.signInButton")}
              </>
            )}
          </button>

          <div className="text-center text-xs text-gray-400">
            {t("partner.login.noAccount")}{" "}
            <Link
              to="/partner/register"
              className="text-primary hover:underline font-semibold inline-flex items-center gap-1"
            >
              {t("partner.login.registerLink")} <ArrowRight size={11} />
            </Link>
          </div>
        </form>

        <div className="text-center mt-6">
          <Link to="/" className="text-xs text-gray-400 hover:text-white transition-colors">
            {t("partner.login.parentLoginLink")}
          </Link>
        </div>
      </motion.div>
    </div>
  );
}
