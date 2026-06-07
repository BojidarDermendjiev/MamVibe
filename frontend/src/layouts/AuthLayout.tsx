import { Navigate, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Sun, Moon } from "lucide-react";
import { useAuthStore } from "../store/authStore";
import { useTheme } from "../contexts/ThemeContext";
import LanguageSwitcher from "../components/common/LanguageSwitcher";
import LoadingSpinner from "../components/common/LoadingSpinner";

const SWITCH_ROUTES = ["/login", "/register"];

export default function AuthLayout() {
  const { t } = useTranslation();
  const { theme, toggleTheme } = useTheme();
  const location = useLocation();
  const navigate = useNavigate();
  const isLogin = location.pathname === "/login";
  const showSwitch = SWITCH_ROUTES.includes(location.pathname);
  const { isAuthenticated, isLoading } = useAuthStore();

  const [transitionOverride, setTransitionOverride] = useState<boolean | null>(null);
  const isActive = transitionOverride ?? !isLogin;
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => () => { if (timerRef.current) clearTimeout(timerRef.current); }, []);

  // While session is resolving, show a spinner so the login form never flashes
  // for users who are already authenticated.
  if (showSwitch && isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div
          className="fixed inset-0 -z-10"
          style={{ background: "linear-gradient(135deg, #945c67 0%, #3f4b7f 100%)" }}
        />
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  // Already logged in — send straight to home instead of showing the form.
  if (showSwitch && isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const switchTo = (path: string, active: boolean) => {
    setTransitionOverride(active);
    // Navigate after animation finishes so the form swap is hidden by the panel
    timerRef.current = setTimeout(() => {
      navigate(path);
      setTransitionOverride(null);
    }, 650);
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      {/* Fixed full-viewport gradient — prevents body background from bleeding
          through the padding corners in light mode */}
      <div
        className="fixed inset-0 -z-10"
        style={{ background: "linear-gradient(135deg, #945c67 0%, #3f4b7f 100%)" }}
      />

      <div className="w-full max-w-[820px] relative">
        <button
          onClick={toggleTheme}
          aria-label={theme === 'dark' ? t('common.switch_to_light', 'Switch to light mode') : t('common.switch_to_dark', 'Switch to dark mode')}
          className="absolute top-4 right-4 z-50 p-2 rounded-full bg-white/10 border border-white/20 backdrop-blur-md hover:bg-white/30 transition-all duration-200 text-gray-700 dark:text-gray-200"
        >
          {theme === 'dark' ? <Sun size={16} /> : <Moon size={16} />}
        </button>
        {showSwitch ? (
          /* ═══════════════════════════════════════
             Auth-switch card
          ═══════════════════════════════════════ */
          <div className={`auth-wrapper${isActive ? " active" : ""}`}>

            {/* ── Form panel (Outlet renders LoginPage / RegisterPage) ── */}
            <div className="auth-form-slot">
              <Outlet />
            </div>

            {/* ── Colored sliding panel (desktop only, hidden on mobile) ── */}
            <div className="auth-toggle-container">
              <div className="auth-toggle-inner">

                {/* Left half — "One of us?" shown when active (register mode) */}
                <div className="auth-toggle-panel auth-toggle-panel-left">
                  <h2 className="text-2xl font-bold mb-3">{t('auth.panel_one_of_us')}</h2>
                  <p className="text-white/80 text-sm leading-relaxed mb-7 max-w-[190px] mx-auto">
                    {t('auth.panel_one_of_us_desc')}
                  </p>
                  <button
                    type="button"
                    onClick={() => switchTo("/login", false)}
                    className="px-9 py-2.5 rounded-full border-2 border-white text-white text-sm font-bold uppercase tracking-widest hover:bg-white hover:text-[#3f4b7f] transition-all duration-300 cursor-pointer"
                  >
                    {t('auth.login_btn')}
                  </button>
                </div>

                {/* Right half — "New here?" shown when not active (login mode) */}
                <div className="auth-toggle-panel auth-toggle-panel-right">
                  <h2 className="text-2xl font-bold mb-3">{t('auth.panel_new_here')}</h2>
                  <p className="text-white/80 text-sm leading-relaxed mb-7 max-w-[190px] mx-auto">
                    {t('auth.panel_new_here_desc')}
                  </p>
                  <button
                    type="button"
                    onClick={() => switchTo("/register", true)}
                    className="px-9 py-2.5 rounded-full border-2 border-white text-white text-sm font-bold uppercase tracking-widest hover:bg-white hover:text-[#3f4b7f] transition-all duration-300 cursor-pointer"
                  >
                    {t('auth.register_btn')}
                  </button>
                </div>

              </div>
            </div>

            {/* ── Mobile switch link ── */}
            <div className="auth-mobile-switch md:hidden absolute bottom-3 left-0 right-0">
              {isLogin ? (
                <p>
                  {t('auth.no_account')}{" "}
                  <button
                    type="button"
                    onClick={() => switchTo("/register", true)}
                    className="text-[#945c67] font-semibold cursor-pointer"
                  >
                    {t('auth.register_btn')}
                  </button>
                </p>
              ) : (
                <p>
                  {t('auth.has_account')}{" "}
                  <button
                    type="button"
                    onClick={() => switchTo("/login", false)}
                    className="text-[#945c67] font-semibold cursor-pointer"
                  >
                    {t('auth.login_btn')}
                  </button>
                </p>
              )}
            </div>

          </div>
        ) : (
          /* ═══════════════════════════════════════
             Simple card (forgot / reset password)
          ═══════════════════════════════════════ */
          <div className="auth-page" style={{ minHeight: '100dvh', position: 'fixed', inset: 0, overflow: 'auto' }}>
            <button
              type="button"
              onClick={() => navigate("/")}
              className="fixed top-5 left-6 z-20 flex items-center gap-2 bg-transparent border-none p-0 cursor-pointer"
            >
              <img src="/logo.png" alt="MamVibe" className="h-9 w-9 object-contain" />
              <span className="text-base font-bold text-[#3f4b7f] dark:text-[#c1c4e3]">MamVibe</span>
            </button>
            <div className="fixed top-5 right-6 z-20 flex items-center gap-2">
              <LanguageSwitcher />
              <button
                type="button"
                onClick={toggleTheme}
                aria-label={theme === "dark" ? t("common.switch_to_light", "Switch to light mode") : t("common.switch_to_dark", "Switch to dark mode")}
                className="p-2 rounded-full bg-black/10 dark:bg-white/10 hover:bg-black/20 dark:hover:bg-white/20 transition-all duration-200 text-gray-700 dark:text-gray-200 border border-black/10 dark:border-white/20"
              >
                {theme === "dark" ? <Sun size={16} /> : <Moon size={16} />}
              </button>
            </div>
            <div style={{ background: '#1e1c2e', borderRadius: '24px', padding: '2.5rem', width: '100%', maxWidth: '420px', boxShadow: '0 5px 45px rgba(0,0,0,0.4)', marginTop: '4rem' }}>
              <Outlet />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
