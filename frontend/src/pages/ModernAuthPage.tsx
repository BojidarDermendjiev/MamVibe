import { useState, useEffect } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import toast from "react-hot-toast";
import { FcGoogle } from "react-icons/fc";
import { HiOutlineUser, HiOutlineLockClosed, HiOutlineMail, HiSun, HiMoon } from "react-icons/hi";
import { authApi } from "../api/authApi";
import { useAuthStore } from "../store/authStore";
import { useTheme } from "../contexts/ThemeContext";
import { ProfileType } from "../types/auth";
import ProfileTypeSelector from "../components/user/ProfileTypeSelector";

export default function ModernAuthPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { setAuth } = useAuthStore();
  const { theme, toggleTheme } = useTheme();
  const [isSignUp, setIsSignUp] = useState(location.pathname === "/register");

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
      toast.success("Welcome back!");
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
        toast.success("Welcome!");
        navigate("/");
      })
      .catch(() => { if (!cancelled) toast.error("Google login failed"); });
    return () => { cancelled = true; };
  }, [navigate, setAuth]);

  const handleGoogleLogin = () => {
    const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
    if (!clientId) { toast.error("Google login is not configured"); return; }
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
      const { data } = await authApi.register(regForm);
      setAuth(data.user, data.accessToken);
      toast.success("Account created!");
      navigate("/");
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg || t("common.error"));
    } finally {
      setRegLoading(false);
    }
  };

  return (
    <div className="auth-page">
      {/* Logo */}
      <Link to="/" className="absolute top-5 left-6 z-20 flex items-center gap-2">
        <img src="/logo.png" alt="MamVibe" className="h-9 w-9 object-contain" />
        <span className="text-base font-bold text-[#3f4b7f] dark:text-[#c1c4e3]">MamVibe</span>
      </Link>

      {/* Theme toggle */}
      <button
        type="button"
        onClick={toggleTheme}
        aria-label="Toggle theme"
        className="absolute top-5 right-6 z-20 w-10 h-10 rounded-full bg-white dark:bg-[#2a2740] shadow-md flex items-center justify-center text-gray-500 dark:text-gray-300 hover:scale-105 transition-transform"
      >
        {theme === "dark" ? <HiSun size={20} /> : <HiMoon size={20} />}
      </button>

      {/* Card */}
      <div className={`auth-card${isSignUp ? " active" : ""}`}>

        {/* ── Login panel (left) ── */}
        <div className="auth-panel auth-panel-login">
          <form onSubmit={handleLogin} className="auth-form-inner">
            <h1 className="auth-title">{t("auth.login_title")}</h1>

            <div className="auth-social-row">
              <button type="button" onClick={handleGoogleLogin} className="auth-social-icon" aria-label="Google">
                <FcGoogle size={20} />
              </button>
            </div>
            <span className="auth-or">{t("auth.login_subtitle")}</span>

            <div className="auth-fields">
              <div className="auth-field">
                <HiOutlineMail className="auth-field-icon" size={16} />
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
                <HiOutlineLockClosed className="auth-field-icon" size={16} />
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

            <Link to="/forgot-password" className="auth-forgot">
              {t("auth.forgot_password")}
            </Link>

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
                <FcGoogle size={20} />
              </button>
            </div>
            <span className="auth-or">{t("auth.register_subtitle")}</span>

            <div className="auth-fields">
              <div className="auth-field">
                <HiOutlineUser className="auth-field-icon" size={16} />
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
                <HiOutlineMail className="auth-field-icon" size={16} />
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
                <HiOutlineLockClosed className="auth-field-icon" size={16} />
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
                <HiOutlineLockClosed className="auth-field-icon" size={16} />
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

            <button type="submit" disabled={regLoading} className="auth-btn-fill">
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
              <h1 className="auth-overlay-title">Welcome Back!</h1>
              <p className="auth-overlay-text">
                To keep connected with us please login with your personal info
              </p>
              <button type="button" onClick={toggleToSignIn} className="auth-btn-ghost">
                {t("auth.login_btn")}
              </button>
            </div>

            {/* Right panel — visible when login is active */}
            <div className="auth-overlay-panel auth-overlay-right">
              <h1 className="auth-overlay-title">Hello, Friend!</h1>
              <p className="auth-overlay-text">
                Enter your personal details and start your journey with us
              </p>
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
