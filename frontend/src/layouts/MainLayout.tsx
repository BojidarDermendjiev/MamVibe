import { useEffect, useRef, useState } from "react";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { clsx } from "clsx";
import {
  Home,
  Search,
  PlusCircle,
  MessageCircle,
  LayoutDashboard,
  MessageSquare,
  Wallet,
  LogIn,
  Sun,
  Moon,
} from "lucide-react";
import { useTheme } from "../contexts/ThemeContext";
import { useAuthStore } from "../store/authStore";
import { authApi } from "../api/authApi";
import { useNotification } from "../contexts/NotificationContext";
import { type NavItem } from "../components/common/TubelightNavBar";
import LanguageSwitcher from "../components/common/LanguageSwitcher";
import Avatar from "../components/common/Avatar";
import Button from "../components/common/Button";
import CookieConsent from "../components/common/CookieConsent";
import ScrollToTop from "../components/common/ScrollToTop";
import toast from "../utils/toast";

export default function MainLayout() {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const { user, isAuthenticated, logout } = useAuthStore();
  const { unreadCount, pendingRequestCount } = useNotification();
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const { theme, toggleTheme } = useTheme();

  // Hide header on scroll-down, reveal on scroll-up
  const [headerVisible, setHeaderVisible] = useState(true);
  const lastScrollY = useRef(0);
  useEffect(() => {
    const onScroll = () => {
      const y = window.scrollY;
      if (y < 10) {
        setHeaderVisible(true);
      } else if (y > lastScrollY.current + 4) {
        setHeaderVisible(false);
      } else if (y < lastScrollY.current - 4) {
        setHeaderVisible(true);
      }
      lastScrollY.current = y;
    };
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  const navItems: NavItem[] = [
    { name: t("nav.home") || "Home", url: "/", icon: Home },
    { name: t("nav.browse") || "Browse", url: "/browse", icon: Search },
    ...(isAuthenticated
      ? [
          {
            name: t("nav.create") || "Create",
            url: "/create",
            icon: PlusCircle,
          },
          {
            name: t("nav.chat") || "Chat",
            url: "/chat",
            icon: MessageCircle,
            badge: unreadCount > 0 ? unreadCount : undefined,
          },
          {
            name: t("nav.dashboard") || "Dashboard",
            url: "/dashboard",
            icon: LayoutDashboard,
            badge: pendingRequestCount > 0 ? pendingRequestCount : undefined,
          },
          {
            name: t("nav.feedback") || "Feedback",
            url: "/feedback",
            icon: MessageSquare,
          },
          {
            name: t("nav.wallet") || "Wallet",
            url: "/wallet",
            icon: Wallet,
          },
        ]
      : []),
  ];

  const getActive = (): string | null => {
    const match = navItems.find(
      (item) =>
        (item.url === "/" && location.pathname === "/") ||
        (item.url !== "/" && location.pathname.startsWith(item.url)),
    );
    return match?.name ?? null;
  };

  const [activeTab, setActiveTab] = useState<string | null>(getActive);

  useEffect(() => {
    // getActive() reads navItems which is derived from isAuthenticated and location.pathname.
    // Both deps are required: pathname drives which tab is active, isAuthenticated controls
    // which nav items exist (authenticated-only items appear on login).
    setActiveTab(getActive());
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname, isAuthenticated]);

  const handleLogout = async () => {
    const name = user?.displayName;
    try { await authApi.revoke(); } catch { /* best effort */ }
    logout();
    navigate("/");
    toast.success(name ? `See you soon, ${name}! 👋` : 'See you soon! 👋');
  };

  /* ─── Shared pill nav (used by both desktop center + mobile bottom) ─── */
  const NavPill = ({ mobile = false }: { mobile?: boolean }) => (
    <div className="flex items-center gap-3 bg-white/10 border border-white/20 backdrop-blur-lg py-1 px-1 rounded-full shadow-lg">
      {navItems.map((item) => {
        const Icon = item.icon;
        const isActive = activeTab === item.name;

        return (
          <Link
            key={item.name}
            to={item.url}
            onClick={() => setActiveTab(item.name)}
            className={clsx(
              "relative cursor-pointer text-sm font-semibold rounded-full transition-colors",
              mobile ? "px-4 py-2" : "px-6 py-2",
              "text-gray-500 hover:text-gray-900",
              isActive && "text-gray-900",
            )}
          >
            {/* Desktop: text label */}
            <span className={mobile ? "hidden" : "hidden md:inline"}>
              {item.name}
            </span>

            {/* Icon (always on mobile pill, md:hidden on desktop) */}
            <span className={clsx("relative", mobile ? "block" : "md:hidden")}>
              <Icon size={18} strokeWidth={2.5} />
              {!!item.badge && item.badge > 0 && (
                <span className="absolute -top-1.5 -right-1.5 bg-red-500 text-white text-[9px] font-bold rounded-full h-3.5 min-w-3.5 flex items-center justify-center px-0.5 leading-none">
                  {item.badge > 9 ? "9+" : item.badge}
                </span>
              )}
            </span>

            {/* Desktop badge on text label */}
            {!mobile && !!item.badge && item.badge > 0 && (
              <span className="hidden md:flex absolute -top-0.5 -right-0.5 bg-red-500 text-white text-[9px] font-bold rounded-full h-3.5 min-w-3.5 items-center justify-center px-0.5 leading-none">
                {item.badge > 9 ? "9+" : item.badge}
              </span>
            )}

            {/* Tubelight glow — exactly as reference */}
            {isActive && (
              <motion.div
                layoutId={mobile ? "lamp-mobile" : "lamp"}
                className="absolute inset-0 w-full bg-primary/5 rounded-full -z-10"
                initial={false}
                transition={{ type: "spring", stiffness: 300, damping: 30 }}
              >
                <div className="absolute -top-2 left-1/2 -translate-x-1/2 w-8 h-1 bg-primary rounded-t-full">
                  <div className="absolute w-12 h-6 bg-primary/20 rounded-full blur-md -top-2 -left-2" />
                  <div className="absolute w-8 h-6 bg-primary/20 rounded-full blur-md -top-1" />
                  <div className="absolute w-4 h-4 bg-primary/20 rounded-full blur-sm top-0 left-2" />
                </div>
              </motion.div>
            )}
          </Link>
        );
      })}
    </div>
  );

  /* ─── Auth controls (shared) ─── */
  const AuthControls = () => (
    <div className="flex items-center gap-2">
      {/* Dark mode toggle */}
      <button
        onClick={toggleTheme}
        title={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}
        aria-label={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}
        className="p-2 rounded-full bg-white/10 border border-white/20 backdrop-blur-md hover:bg-white/20 transition-colors text-gray-700 dark:text-gray-200"
      >
        {theme === "dark" ? <Sun size={16} /> : <Moon size={16} />}
      </button>
      <LanguageSwitcher />

      {isAuthenticated ? (
        <>
          {/* Avatar dropdown */}
          <div className="relative">
            <button
              onClick={() => setDropdownOpen((v) => !v)}
              className="flex items-center gap-1.5 p-1 pr-3 rounded-full bg-white/10 border border-white/20 backdrop-blur-md hover:bg-white/20 transition-colors relative z-50"
            >
              <Avatar
                src={user?.avatarUrl}
                profileType={user?.profileType}
                size="sm"
              />
              <span className="hidden sm:block text-sm font-medium text-gray-700 max-w-[90px] truncate">
                {user?.displayName}
              </span>
            </button>

            {dropdownOpen && (
              <div className="absolute right-0 mt-2 w-52 bg-white dark:bg-[#2d2a42] rounded-xl shadow-lg border border-lavender/30 py-2 z-50">
                <Link
                  to="/profile"
                  className="block px-4 py-2 text-sm text-gray-700 dark:text-gray-100 hover:bg-cream-dark dark:hover:bg-white/10"
                  onClick={() => setDropdownOpen(false)}
                >
                  {t("nav.profile")}
                </Link>
                <Link
                  to="/settings"
                  className="block px-4 py-2 text-sm text-gray-700 dark:text-gray-100 hover:bg-cream-dark dark:hover:bg-white/10"
                  onClick={() => setDropdownOpen(false)}
                >
                  {t("nav.settings")}
                </Link>
                {user?.roles.includes("Admin") && (
                  <Link
                    to="/admin"
                    className="block px-4 py-2 text-sm text-mauve dark:text-purple-300 font-medium hover:bg-cream-dark dark:hover:bg-white/10"
                    onClick={() => setDropdownOpen(false)}
                  >
                    {t("nav.admin")}
                  </Link>
                )}
                <hr className="my-1 border-lavender/30 dark:border-white/10" />
                <button
                  onClick={() => {
                    handleLogout();
                    setDropdownOpen(false);
                  }}
                  className="w-full text-left px-4 py-2 text-sm text-red-500 dark:text-red-400 hover:bg-cream-dark dark:hover:bg-white/10"
                >
                  {t("nav.logout")}
                </button>
              </div>
            )}
          </div>
        </>
      ) : (
        <Button
          size="sm"
          variant="primary"
          onClick={() => navigate("/login")}
          className="rounded-full gap-1.5"
        >
          <LogIn size={14} />
          <span className="hidden sm:inline">
            {t("nav.login") || "Sign In"}
          </span>
        </Button>
      )}
    </div>
  );

  return (
    <div className="min-h-screen flex flex-col bg-white dark:bg-[#1a1825] transition-colors duration-300">
      {/* Full-screen overlay — closes dropdown when clicking anywhere on the page */}
      {dropdownOpen && (
        <div
          className="fixed inset-0 z-40"
          onClick={() => setDropdownOpen(false)}
          aria-hidden="true"
        />
      )}

      {/* ══════════════════════════════════════════════════════
          DESKTOP  ≥ md
          Three floating elements at top — exactly like reference:
            left: logo pill  |  center: tubelight nav pill  |  right: auth controls
          All use pointer-events-none on wrapper so page is still scrollable
      ══════════════════════════════════════════════════════ */}
      <div
        className={clsx(
          "hidden md:block fixed top-0 left-0 right-0 z-50 pointer-events-none",
          "transition-transform duration-300 ease-in-out",
          headerVisible ? "translate-y-0" : "-translate-y-full",
        )}
      >
        <div className="relative flex items-start px-8 pt-6">
          {/* Logo — top left */}
          <Link
            to="/"
            className="pointer-events-auto flex items-center gap-2 bg-white/10 border border-white/20 backdrop-blur-lg rounded-full px-4 py-2 shadow-lg hover:bg-white/20 transition-colors"
          >
            <img
              src="/logo.png"
              alt="MamVibe"
              className="h-7 w-7 object-contain"
            />
            <span className="text-sm font-bold text-gray-800">MamVibe</span>
          </Link>

          {/* Tubelight pill — perfectly centered (absolute) */}
          <div className="pointer-events-auto absolute left-1/2 top-6 -translate-x-1/2">
            <NavPill />
          </div>

          {/* Auth — top right (ml-auto pushes it right) */}
          <div className="pointer-events-auto ml-auto">
            <AuthControls />
          </div>
        </div>
      </div>

      {/* ══════════════════════════════════════════════════════
          MOBILE  < md
          Solid mini header (logo + auth) at top
      ══════════════════════════════════════════════════════ */}
      <header
        className={clsx(
          "md:hidden fixed top-0 left-0 right-0 z-50 h-14",
          "bg-white/90 backdrop-blur-md border-b border-gray-100",
          "flex items-center justify-between px-4",
          "transition-transform duration-300 ease-in-out",
          headerVisible ? "translate-y-0" : "-translate-y-full",
        )}
      >
        <Link to="/" className="flex items-center gap-2">
          <img
            src="/logo.png"
            alt="MamVibe"
            className="h-[38px] w-[38px] object-contain"
          />
          <span className="text-base font-bold text-gray-800">MamVibe</span>
        </Link>
        <AuthControls />
      </header>

      {/* ══════════════════════════════════════════════════════
          MOBILE bottom floating pill — exactly like reference
          fixed bottom-0 … mb-6
      ══════════════════════════════════════════════════════ */}
      <div className="md:hidden fixed bottom-0 left-1/2 -translate-x-1/2 z-50 mb-6">
        <NavPill mobile />
      </div>

      {/* ── Main content ── */}
      {/*
        Desktop: pt-20 clears the top floating row (pt-6 + ~44px pill + 8px gap ≈ 78px → pt-20)
        Mobile:  pt-16 clears the solid top bar; pb-24 clears the bottom pill
      */}
      <main className="flex-1 animate-fade-in pt-14 pb-24 md:pt-20 md:pb-0">
        <Outlet />
      </main>

      {/* ── Footer ── */}
      <footer className="bg-white dark:bg-[#1a1825] border-t border-gray-200 dark:border-gray-700/60 py-10 mt-auto transition-colors duration-300">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            <div>
              <h3 className="text-lg font-bold mb-3 flex items-center gap-2 text-gray-800 dark:text-gray-100">
                <img
                  src="/logo.png"
                  alt="MamVibe"
                  className="h-6 w-6 object-contain"
                />
                {t("footer.about")}
              </h3>
              <p className="text-gray-500 dark:text-gray-400 text-sm">
                {t("footer.about_text")}
              </p>
            </div>
            <div>
              <h3 className="text-lg font-bold mb-3 text-gray-800 dark:text-gray-100">{t("footer.links")}</h3>
              <div className="space-y-2 text-sm">
                <Link
                  to="/browse"
                  className="block text-gray-500 dark:text-gray-400 hover:text-primary dark:hover:text-gray-100 transition-colors"
                >
                  {t("nav.browse")}
                </Link>
                {!isAuthenticated && (
                  <Link
                    to="/register"
                    className="block text-gray-500 dark:text-gray-400 hover:text-primary dark:hover:text-gray-100 transition-colors"
                  >
                    {t("nav.register")}
                  </Link>
                )}
              </div>
            </div>
            <div>
              <h3 className="text-lg font-bold mb-3 text-gray-800 dark:text-gray-100">{t("footer.contact")}</h3>
              <p className="text-gray-500 dark:text-gray-400 text-sm">support@mamvibe.com</p>
            </div>

            {/* Download the app */}
            <div className="rounded-2xl bg-gray-100 dark:bg-[#2d2a42] p-5 flex flex-col gap-4">
              <h3 className="text-base font-bold text-cyan-500 dark:text-cyan-400">{t("footer.download")}</h3>
              <div className="flex flex-col gap-3">
                {/* App Store badge */}
                <a
                  href="#"
                  aria-label={t("footer.app_store")}
                  className="flex items-center gap-3 border border-gray-300 dark:border-white/20 rounded-xl px-4 py-2.5 hover:bg-gray-200 dark:hover:bg-white/10 transition-colors"
                >
                  <svg viewBox="0 0 24 24" className="h-6 w-6 fill-gray-800 dark:fill-white flex-shrink-0" xmlns="http://www.w3.org/2000/svg">
                    <path d="M18.71 19.5c-.83 1.24-1.71 2.45-3.05 2.47-1.34.03-1.77-.79-3.29-.79-1.53 0-2 .77-3.27.82-1.31.05-2.3-1.32-3.14-2.53C4.25 17 2.94 12.45 4.7 9.39c.87-1.52 2.43-2.48 4.12-2.51 1.28-.02 2.5.87 3.29.87.78 0 2.26-1.07 3.8-.91.65.03 2.47.26 3.64 1.98-.09.06-2.17 1.28-2.15 3.81.03 3.02 2.65 4.03 2.68 4.04-.03.07-.42 1.44-1.38 2.83M13 3.5c.73-.83 1.94-1.46 2.94-1.5.13 1.17-.34 2.35-1.04 3.19-.69.85-1.83 1.51-2.95 1.42-.15-1.15.41-2.35 1.05-3.11z"/>
                  </svg>
                  <div className="leading-tight">
                    <div className="text-gray-500 dark:text-white/60 text-[10px]">Download on the</div>
                    <div className="text-gray-800 dark:text-white font-semibold text-sm">App Store</div>
                  </div>
                </a>

                {/* Google Play badge */}
                <a
                  href="#"
                  aria-label={t("footer.google_play")}
                  className="flex items-center gap-3 border border-gray-300 dark:border-white/20 rounded-xl px-4 py-2.5 hover:bg-gray-200 dark:hover:bg-white/10 transition-colors"
                >
                  <svg viewBox="0 0 24 24" className="h-6 w-6 flex-shrink-0" xmlns="http://www.w3.org/2000/svg">
                    <path d="M3 20.5v-17c0-.83 1-.83 1.5-.5l15 8.5-15 8.5c-.5.33-1.5.33-1.5-.5z" fill="#EA4335"/>
                    <path d="M3 3.5l9 9-9 8V3.5z" fill="#FBBC04"/>
                    <path d="M3 3.5l9 9 6-6-13.5-3.5z" fill="#4285F4"/>
                    <path d="M3 20.5l9-9 6 6-13.5 3.5z" fill="#34A853"/>
                  </svg>
                  <div className="leading-tight">
                    <div className="text-gray-500 dark:text-white/60 text-[10px]">GET IT ON</div>
                    <div className="text-gray-800 dark:text-white font-semibold text-sm">Google Play</div>
                  </div>
                </a>
              </div>
            </div>
          </div>

          <div className="border-t border-gray-200 dark:border-gray-700/60 mt-8 pt-6 text-center text-sm text-gray-400 dark:text-gray-500">
            &copy; {new Date().getFullYear()} MamVibe. {t("footer.rights")}
          </div>
        </div>
      </footer>

      <CookieConsent />
      <ScrollToTop />
    </div>
  );
}
