import { Link, Navigate, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useEffect, useRef, useState } from "react";
import { Sun, Moon } from "lucide-react";
import { useAuthStore } from "../store/authStore";
import { useTheme } from "../contexts/ThemeContext";
import LoadingSpinner from "../components/common/LoadingSpinner";

const SWITCH_ROUTES = ["/login", "/register"];

export default function AuthLayout() {
  const location = useLocation();
  const navigate = useNavigate();
  const isLogin = location.pathname === "/login";
  const showSwitch = SWITCH_ROUTES.includes(location.pathname);
  const { isAuthenticated, isLoading } = useAuthStore();

  // Must be declared before any early returns (Rules of Hooks)
  const { theme, toggleTheme } = useTheme();

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

      {/* Theme toggle — top-right corner */}
      <button
        onClick={toggleTheme}
        aria-label={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}
        title={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}
        className="fixed top-4 right-4 z-50 p-2.5 rounded-full bg-white/20 backdrop-blur-sm border border-white/30 text-white hover:bg-white/30 transition-colors"
      >
        {theme === "dark" ? <Sun size={18} /> : <Moon size={18} />}
      </button>
      <div className="w-full max-w-[820px]">
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
                  <h2 className="text-2xl font-bold mb-3">One of us?</h2>
                  <p className="text-white/80 text-sm leading-relaxed mb-7 max-w-[190px] mx-auto">
                    Welcome back! Sign in to continue your journey with us.
                  </p>
                  <button
                    type="button"
                    onClick={() => switchTo("/login", false)}
                    className="px-9 py-2.5 rounded-full border-2 border-white text-white text-sm font-bold uppercase tracking-widest hover:bg-white hover:text-[#3f4b7f] transition-all duration-300 cursor-pointer"
                  >
                    Sign In
                  </button>
                </div>

                {/* Right half — "New here?" shown when not active (login mode) */}
                <div className="auth-toggle-panel auth-toggle-panel-right">
                  <h2 className="text-2xl font-bold mb-3">New here?</h2>
                  <p className="text-white/80 text-sm leading-relaxed mb-7 max-w-[190px] mx-auto">
                    Join us today and discover a world of possibilities.
                    Create your account in seconds!
                  </p>
                  <button
                    type="button"
                    onClick={() => switchTo("/register", true)}
                    className="px-9 py-2.5 rounded-full border-2 border-white text-white text-sm font-bold uppercase tracking-widest hover:bg-white hover:text-[#3f4b7f] transition-all duration-300 cursor-pointer"
                  >
                    Sign Up
                  </button>
                </div>

              </div>
            </div>

            {/* ── Mobile switch link ── */}
            <div className="auth-mobile-switch md:hidden absolute bottom-3 left-0 right-0">
              {isLogin ? (
                <p>
                  Don't have an account?{" "}
                  <button
                    type="button"
                    onClick={() => switchTo("/register", true)}
                    className="text-[#945c67] font-semibold cursor-pointer"
                  >
                    Sign Up
                  </button>
                </p>
              ) : (
                <p>
                  Already have an account?{" "}
                  <button
                    type="button"
                    onClick={() => switchTo("/login", false)}
                    className="text-[#945c67] font-semibold cursor-pointer"
                  >
                    Sign In
                  </button>
                </p>
              )}
            </div>

          </div>
        ) : (
          /* ═══════════════════════════════════════
             Simple card (forgot / reset password)
          ═══════════════════════════════════════ */
          <div className="bg-white rounded-2xl shadow-2xl p-8 max-w-sm mx-auto">
            <div className="text-center mb-6">
              <Link to="/" className="inline-flex flex-col items-center gap-1">
                <img src="/logo.png" alt="MamVibe" className="h-12 w-12 object-contain" />
                <span className="text-lg font-bold text-primary">MamVibe</span>
              </Link>
            </div>
            <Outlet />
          </div>
        )}
      </div>
    </div>
  );
}
