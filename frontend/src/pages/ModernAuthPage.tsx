import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import toast from "react-hot-toast";
import { clsx } from "clsx";
import { FcGoogle } from "react-icons/fc";
import { authApi } from "../api/authApi";
import { useAuthStore } from "../store/authStore";
import { ProfileType } from "../types/auth";
import ProfileTypeSelector from "../components/user/ProfileTypeSelector";

export default function ModernAuthPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { setAuth } = useAuthStore();
  const [isSignUp, setIsSignUp] = useState(location.pathname === "/register");

  // Login state
  const [loginEmail, setLoginEmail] = useState("");
  const [loginPassword, setLoginPassword] = useState("");
  const [loginLoading, setLoginLoading] = useState(false);

  // Register state
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
      const { data } = await authApi.login({
        email: loginEmail,
        password: loginPassword,
      });
      setAuth(data.user, data.accessToken, data.refreshToken);
      toast.success("Welcome back!");
      navigate("/");
    } catch (err: unknown) {
      const msg = (
        err as { response?: { data?: { message?: string } } }
      )?.response?.data?.message;
      toast.error(msg || t("common.error"));
    } finally {
      setLoginLoading(false);
    }
  };

  const handleGoogleLogin = () => {
    if (typeof google !== "undefined") {
      google.accounts.id.initialize({
        client_id: import.meta.env.VITE_GOOGLE_CLIENT_ID || "",
        callback: async (response: { credential: string }) => {
          try {
            const { data } = await authApi.googleLogin({
              idToken: response.credential,
            });
            setAuth(data.user, data.accessToken, data.refreshToken);
            toast.success("Welcome!");
            navigate("/");
          } catch {
            toast.error("Google login failed");
          }
        },
      });
      google.accounts.id.prompt();
    }
  };

  const validate = () => {
    const errs: Record<string, string> = {};
    if (regForm.password.length < 8)
      errs.password = t("auth.password_min_length");
    else if (!/[A-Z]/.test(regForm.password))
      errs.password = t("auth.password_uppercase");
    else if (!/[a-z]/.test(regForm.password))
      errs.password = t("auth.password_lowercase");
    else if (!/[0-9]/.test(regForm.password))
      errs.password = t("auth.password_digit");
    if (regForm.password !== regForm.confirmPassword)
      errs.confirmPassword = t("auth.passwords_no_match");
    if (!regForm.displayName.trim())
      errs.displayName = t("auth.display_name_required");
    setRegErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    setRegLoading(true);
    try {
      const { data } = await authApi.register(regForm);
      setAuth(data.user, data.accessToken, data.refreshToken);
      toast.success("Account created!");
      navigate("/");
    } catch (err: unknown) {
      const msg = (
        err as { response?: { data?: { message?: string } } }
      )?.response?.data?.message;
      toast.error(msg || t("common.error"));
    } finally {
      setRegLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-r from-peach to-lavender-light flex items-center justify-center p-4">
      {/* Logo */}
      <Link
        to="/"
        className="absolute top-6 left-6 inline-flex items-center gap-2 z-10"
      >
        <img
          src="/logo.png"
          alt="MamVibe"
          className="h-12 w-12 object-contain"
        />
        <span className="text-xl font-bold text-primary">MamVibe</span>
      </Link>

      {/* Container */}
      <div
        className={clsx(
          "modern-auth-container",
          isSignUp && "active",
        )}
      >
        {/* ── Sign Up Form ── */}
        <div className="modern-auth-form modern-auth-signup">
          <form onSubmit={handleRegister}>
            <h1 className="text-2xl font-bold text-primary-dark">
              {t("auth.register_title")}
            </h1>
            <span className="text-xs text-gray-400 mt-2 mb-4 block">
              {t("auth.register_subtitle")}
            </span>

            <div className="space-y-2 w-full">
              <input
                type="text"
                placeholder={t("auth.display_name")}
                value={regForm.displayName}
                onChange={(e) =>
                  setRegForm({ ...regForm, displayName: e.target.value })
                }
                className="modern-auth-input"
                required
              />
              {regErrors.displayName && (
                <p className="text-xs text-red-500">{regErrors.displayName}</p>
              )}
              <input
                type="email"
                placeholder={t("auth.email")}
                value={regForm.email}
                onChange={(e) =>
                  setRegForm({ ...regForm, email: e.target.value })
                }
                className="modern-auth-input"
                required
              />
              <input
                type="password"
                placeholder={t("auth.password")}
                value={regForm.password}
                onChange={(e) =>
                  setRegForm({ ...regForm, password: e.target.value })
                }
                className="modern-auth-input"
                required
              />
              {regErrors.password && (
                <p className="text-xs text-red-500">{regErrors.password}</p>
              )}
              <input
                type="password"
                placeholder={t("auth.confirm_password")}
                value={regForm.confirmPassword}
                onChange={(e) =>
                  setRegForm({ ...regForm, confirmPassword: e.target.value })
                }
                className="modern-auth-input"
                required
              />
              {regErrors.confirmPassword && (
                <p className="text-xs text-red-500">
                  {regErrors.confirmPassword}
                </p>
              )}
              <ProfileTypeSelector
                value={regForm.profileType}
                onChange={(profileType) =>
                  setRegForm({ ...regForm, profileType })
                }
              />
            </div>

            <button
              type="submit"
              disabled={regLoading}
              className="modern-auth-btn mt-3"
            >
              {regLoading ? t("common.loading") : t("auth.register_btn")}
            </button>

            {/* Mobile-only toggle link */}
            <p className="text-center text-sm text-gray-500 mt-4 md:hidden">
              {t("auth.has_account")}{" "}
              <button
                type="button"
                onClick={toggleToSignIn}
                className="text-primary font-medium hover:underline"
              >
                {t("nav.login")}
              </button>
            </p>
          </form>
        </div>

        {/* ── Sign In Form ── */}
        <div className="modern-auth-form modern-auth-signin">
          <form onSubmit={handleLogin}>
            <h1 className="text-2xl font-bold text-primary-dark">
              {t("auth.login_title")}
            </h1>

            {/* Google social login */}
            <div className="flex gap-3 my-5">
              <button
                type="button"
                onClick={handleGoogleLogin}
                className="modern-auth-social-icon"
              >
                <FcGoogle size={20} />
              </button>
            </div>

            <span className="text-xs text-gray-400 mb-4 block">
              {t("auth.login_subtitle")}
            </span>

            <div className="space-y-3 w-full">
              <input
                type="email"
                placeholder={t("auth.email")}
                value={loginEmail}
                onChange={(e) => setLoginEmail(e.target.value)}
                className="modern-auth-input"
                required
              />
              <input
                type="password"
                placeholder={t("auth.password")}
                value={loginPassword}
                onChange={(e) => setLoginPassword(e.target.value)}
                className="modern-auth-input"
                required
              />
            </div>

            <Link
              to="/forgot-password"
              className="text-sm text-gray-500 my-4 block hover:text-primary transition-colors"
            >
              {t("auth.forgot_password")}
            </Link>

            <button
              type="submit"
              disabled={loginLoading}
              className="modern-auth-btn"
            >
              {loginLoading ? t("common.loading") : t("auth.login_btn")}
            </button>

            {/* Mobile-only toggle link */}
            <p className="text-center text-sm text-gray-500 mt-4 md:hidden">
              {t("auth.no_account")}{" "}
              <button
                type="button"
                onClick={toggleToSignUp}
                className="text-primary font-medium hover:underline"
              >
                {t("nav.register")}
              </button>
            </p>
          </form>
        </div>

        {/* ── Toggle Container (desktop only) ── */}
        <div className="modern-auth-toggle-container">
          <div className="modern-auth-toggle">
            {/* Left panel — visible when Sign Up is active */}
            <div className="modern-auth-toggle-panel modern-auth-toggle-left">
              <h1 className="text-2xl font-bold">{t("auth.login_title")}</h1>
              <p className="text-sm leading-5 tracking-wide my-5 opacity-90">
                {t("auth.login_subtitle")}
              </p>
              <button
                type="button"
                onClick={toggleToSignIn}
                className="modern-auth-toggle-btn"
              >
                {t("auth.login_btn")}
              </button>
            </div>

            {/* Right panel — visible when Sign In is active */}
            <div className="modern-auth-toggle-panel modern-auth-toggle-right">
              <h1 className="text-2xl font-bold">{t("auth.register_title")}</h1>
              <p className="text-sm leading-5 tracking-wide my-5 opacity-90">
                {t("auth.register_subtitle")}
              </p>
              <button
                type="button"
                onClick={toggleToSignUp}
                className="modern-auth-toggle-btn"
              >
                {t("auth.register_btn")}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

