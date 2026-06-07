import { useState, useEffect, useCallback } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { usePageSEO } from "@/hooks/useSEO";
import { useTheme } from "@/contexts/ThemeContext";
import toast from "react-hot-toast";
import { User, Lock, Mail, Sun, Moon } from "lucide-react";

// Inline Google logo SVG — keeps the brand colour without react-icons/fc
function GoogleIcon({ size = 20 }: { size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" aria-hidden="true">
      <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
      <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
      <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z" fill="#FBBC05"/>
      <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
    </svg>
  );
}
import { authApi } from "../api/authApi";
import { useAuthStore } from "../store/authStore";
import { ProfileType } from "../types/auth";
import ProfileTypeSelector from "../components/user/ProfileTypeSelector";
import TurnstileWidget from "../components/common/TurnstileWidget";

export default function ModernAuthPage() {
  const { t } = useTranslation();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const { setAuth } = useAuthStore();
  const [isSignUp, setIsSignUp] = useState(location.pathname === "/register");

  // Auth pages: noindex to prevent thin/duplicate content in the index.
  usePageSEO({
    title: isSignUp ? "Create Your Free Account" : "Sign In to MamVibe",
    description: isSignUp
      ? "Join MamVibe for free. Buy, sell, or donate second-hand baby items with families across Bulgaria."
      : "Sign in to your MamVibe account to manage your listings, messages, and purchases.",
    canonical: isSignUp ? "https://mamvibe.com/register" : "https://mamvibe.com/login",
    index: false,
  });

  const [loginEmail, setLoginEmail] = useState("");
  const [loginPassword, setLoginPassword] = useState("");
  const [loginLoading, setLoginLoading] = useState(false);

  const [regForm, setRegForm] = useState({
    email: "",
    password: "",
    confirmPassword: "",
    displayName: "",
    profileType: ProfileType.Female as ProfileType,
  });
  const [regLoading, setRegLoading] = useState(false);
  const [regErrors, setRegErrors] = useState<Record<string, string>>({});
  const [regToken, setRegToken] = useState<string | null>(null);
  const clearRegToken = useCallback(() => setRegToken(null), []);

  const toggleToSignUp = () => {
    setIsSignUp(true);
    window.history.replaceState(null, "", "/register");
  };

  const toggleToSignIn = () => {
    setIsSignUp(false);
    window.history.replaceState(null, "", "/login");
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoginLoading(true);
    try {
      const { data } = await authApi.login({ email: loginEmail, password: loginPassword });
      setAuth(data.user, data.accessToken);
      toast.success(t("auth.welcome_back"));
      navigate("/");
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t("common.error"));
    } finally {
      setLoginLoading(false);
    }
  };

  // On mount: check if Google redirected back with an id_token in the hash
  useEffect(() => {
    const hash = new URLSearchParams(window.location.hash.replace(/^#/, ""));
    const idToken = hash.get("id_token");
    if (!idToken) return;
    window.history.replaceState(null, "", window.location.pathname);
    let cancelled = false;
    authApi
      .googleLogin({ idToken })
      .then(({ data }) => {
        if (cancelled) return;
        setAuth(data.user, data.accessToken);
        toast.success(t("auth.welcome"));
        navigate("/");
      })
      .catch(() => { if (!cancelled) toast.error(t("auth.google_login_failed")); });
    return () => { cancelled = true; };
  }, [navigate, setAuth]);

  const handleGoogleLogin = () => {
    const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
    if (!clientId) { toast.error(t("auth.google_not_configured")); return; }
    const nonce = crypto.randomUUID();
    sessionStorage.setItem("google_nonce", nonce);
    const params = new URLSearchParams({
      client_id: clientId,
      redirect_uri: `${window.location.origin}/login`,
      response_type: "id_token",
      scope: "openid email profile",
      nonce,
      prompt: "select_account",
    });
    window.location.href = `https://accounts.google.com/o/oauth2/v2/auth?${params}`;
  };

  const validate = () => {
    const errs: Record<string, string> = {};
    if (regForm.password.length < 8) errs.password = t("auth.password_min_length");
    else if (!/[A-Z]/.test(regForm.password)) errs.password = t("auth.password_uppercase");
    else if (!/[a-z]/.test(regForm.password)) errs.password = t("auth.password_lowercase");
    else if (!/[0-9]/.test(regForm.password)) errs.password = t("auth.password_digit");
    if (regForm.password !== regForm.confirmPassword)
      errs.confirmPassword = t("auth.passwords_no_match");
    if (!regForm.displayName.trim()) errs.displayName = t("auth.display_name_required");
    setRegErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    setRegLoading(true);
    try {
      const { data } = await authApi.register({ ...regForm, turnstileToken: regToken ?? undefined });
      setAuth(data.user, data.accessToken);
      toast.success(t("auth.account_created"));
      navigate("/");
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t("common.error"));
      clearRegToken();
    } finally {
      setRegLoading(false);
    }
  };

  return (
    <div className="auth-page">
      {/* Logo */}
      <button
        type="button"
        onClick={() => navigate("/")}
        className="absolute top-5 left-6 z-20 flex items-center gap-2 bg-transparent border-none p-0 cursor-pointer"
      >
        <img src="/logo.png" alt="MamVibe" className="h-9 w-9 object-contain" />
        <span className="text-base font-bold text-[#3f4b7f] dark:text-[#c1c4e3]">MamVibe</span>
      </button>

      {/* Theme toggle */}
      <button
        type="button"
        onClick={toggleTheme}
        aria-label={theme === "dark" ? t("common.switch_to_light", "Switch to light mode") : t("common.switch_to_dark", "Switch to dark mode")}
        className="absolute top-5 right-6 z-20 p-2 rounded-full bg-black/10 dark:bg-white/10 hover:bg-black/20 dark:hover:bg-white/20 transition-all duration-200 text-gray-700 dark:text-gray-200 border border-black/10 dark:border-white/20"
      >
        {theme === "dark" ? <Sun size={16} /> : <Moon size={16} />}
      </button>

      {/* Card */}
      <div className={`auth-card${isSignUp ? " active" : ""}`}>

        {/* ── Login panel (left) ── */}
        <div className="auth-panel auth-panel-login">
          <form onSubmit={handleLogin} className="auth-form-inner">
            <h1 className="auth-title">{t("auth.login_title")}</h1>

            <div className="auth-social-row">
              <button type="button" onClick={handleGoogleLogin} className="auth-social-icon" aria-label="Google">
                <GoogleIcon size={20} />
              </button>
            </div>
            <span className="auth-or">{t("auth.login_subtitle")}</span>

            <div className="auth-fields">
              <div className="auth-field">
                <Mail className="auth-field-icon" size={16} />
                <input
                  type="email"
                  placeholder={t("auth.email")}
                  value={loginEmail}
                  onChange={(e) => setLoginEmail(e.target.value)}
                  className="auth-field-input"
                  required
                />
              </div>
              <div className="auth-field">
                <Lock className="auth-field-icon" size={16} />
                <input
                  type="password"
                  placeholder={t("auth.password")}
                  value={loginPassword}
                  onChange={(e) => setLoginPassword(e.target.value)}
                  className="auth-field-input"
                  required
                />
              </div>
            </div>

            <button
              type="button"
              onClick={() => navigate("/forgot-password")}
              className="auth-forgot bg-transparent border-none p-0 cursor-pointer"
            >
              {t("auth.forgot_password")}
            </button>

            <button type="submit" disabled={loginLoading} className="auth-btn-fill">
              {loginLoading ? t("common.loading") : t("auth.login_btn")}
            </button>

            {/* Mobile-only toggle */}
            <p className="auth-mobile-link">
              {t("auth.no_account")}{" "}
              <button type="button" onClick={toggleToSignUp} className="auth-mobile-link-btn">
                {t("nav.register")}
              </button>
            </p>
          </form>
        </div>

        {/* ── Signup panel (right) ── */}
        <div className="auth-panel auth-panel-signup">
          <form onSubmit={handleRegister} className="auth-form-inner">
            <h1 className="auth-title">{t("auth.register_title")}</h1>

            <div className="auth-social-row">
              <button type="button" onClick={handleGoogleLogin} className="auth-social-icon" aria-label="Google">
                <GoogleIcon size={20} />
              </button>
            </div>
            <span className="auth-or">{t("auth.register_subtitle")}</span>

            <div className="auth-fields">
              <div className="auth-field">
                <User className="auth-field-icon" size={16} />
                <input
                  type="text"
                  placeholder={t("auth.display_name")}
                  value={regForm.displayName}
                  onChange={(e) => setRegForm({ ...regForm, displayName: e.target.value })}
                  className="auth-field-input"
                  required
                />
              </div>
              {regErrors.displayName && <p className="auth-error">{regErrors.displayName}</p>}

              <div className="auth-field">
                <Mail className="auth-field-icon" size={16} />
                <input
                  type="email"
                  placeholder={t("auth.email")}
                  value={regForm.email}
                  onChange={(e) => setRegForm({ ...regForm, email: e.target.value })}
                  className="auth-field-input"
                  required
                />
              </div>

              <div className="auth-field">
                <Lock className="auth-field-icon" size={16} />
                <input
                  type="password"
                  placeholder={t("auth.password")}
                  value={regForm.password}
                  onChange={(e) => setRegForm({ ...regForm, password: e.target.value })}
                  className="auth-field-input"
                  required
                />
              </div>
              {regErrors.password && <p className="auth-error">{regErrors.password}</p>}

              <div className="auth-field">
                <Lock className="auth-field-icon" size={16} />
                <input
                  type="password"
                  placeholder={t("auth.confirm_password")}
                  value={regForm.confirmPassword}
                  onChange={(e) => setRegForm({ ...regForm, confirmPassword: e.target.value })}
                  className="auth-field-input"
                  required
                />
              </div>
              {regErrors.confirmPassword && <p className="auth-error">{regErrors.confirmPassword}</p>}
            </div>

            <div className="auth-profile-selector">
              <ProfileTypeSelector
                value={regForm.profileType}
                onChange={(profileType) => setRegForm({ ...regForm, profileType })}
              />
            </div>

            <div className="flex justify-center my-2">
              <TurnstileWidget onToken={setRegToken} onExpire={clearRegToken} />
            </div>

            <button type="submit" disabled={regLoading || !regToken} className="auth-btn-fill">
              {regLoading ? t("common.loading") : t("auth.register_btn")}
            </button>

            {/* Mobile-only toggle */}
            <p className="auth-mobile-link">
              {t("auth.has_account")}{" "}
              <button type="button" onClick={toggleToSignIn} className="auth-mobile-link-btn">
                {t("nav.login")}
              </button>
            </p>
          </form>
        </div>

        {/* ── Sliding overlay panel (desktop) ── */}
        <div className="auth-overlay">
          <div className="auth-overlay-inner">
            {/* Left panel — visible when signup is active */}
            <div className="auth-overlay-panel auth-overlay-left">
              <h1 className="auth-overlay-title">{t("auth.panel_one_of_us")}</h1>
              <p className="auth-overlay-text">{t("auth.panel_one_of_us_desc")}</p>
              <button type="button" onClick={toggleToSignIn} className="auth-btn-ghost">
                {t("auth.login_btn")}
              </button>
            </div>

            {/* Right panel — visible when login is active */}
            <div className="auth-overlay-panel auth-overlay-right">
              <h1 className="auth-overlay-title">{t("auth.panel_new_here")}</h1>
              <p className="auth-overlay-text">{t("auth.panel_new_here_desc")}</p>
              <button type="button" onClick={toggleToSignUp} className="auth-btn-ghost">
                {t("auth.register_btn")}
              </button>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}
