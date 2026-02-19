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
  ShoppingCart,
  LogIn,
  Sun,
  Moon,
} from "lucide-react";
import { useTheme } from "../contexts/ThemeContext";
import { useAuthStore } from "../store/authStore";
import { useCartStore } from "../store/cartStore";
import { useNotification } from "../contexts/NotificationContext";
import { type NavItem } from "../components/common/TubelightNavBar";
import LanguageSwitcher from "../components/common/LanguageSwitcher";
import Avatar from "../components/common/Avatar";
import Button from "../components/common/Button";
import CookieConsent from "../components/common/CookieConsent";
import ScrollToTop from "../components/common/ScrollToTop";

export default function MainLayout() {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const { user, isAuthenticated, logout } = useAuthStore();
  const { unreadCount } = useNotification();
  const cartItems = useCartStore((s) => s.items);
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
          { name: t("nav.create") || "Create", url: "/create", icon: PlusCircle },
          {
            name: t("nav.chat") || "Chat",
            url: "/chat",
            icon: MessageCircle,
            badge: unreadCount > 0 ? unreadCount : undefined,
          },
          { name: t("nav.dashboard") || "Dashboard", url: "/dashboard", icon: LayoutDashboard },
          { name: t("nav.feedback") || "Feedback", url: "/feedback", icon: MessageSquare },
        ]
      : []),
  ];

  const getActive = () => {
    const match = navItems.find(
      (item) =>
        (item.url === "/" && location.pathname === "/") ||
        (item.url !== "/" && location.pathname.startsWith(item.url))
    );
    return match?.name ?? navItems[0].name;
  };

  const [activeTab, setActiveTab] = useState(getActive);

  useEffect(() => {
    setActiveTab(getActive());
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname, isAuthenticated]);

  const handleLogout = () => {
    logout();
    navigate("/");
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
              isActive && "text-gray-900"
            )}
          >
            {/* Desktop: text label */}
            <span className={mobile ? "hidden" : "hidden md:inline"}>{item.name}</span>

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
        title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
        className="p-2 rounded-full bg-white/10 border border-white/20 backdrop-blur-md hover:bg-white/20 transition-colors text-gray-700 dark:text-gray-200"
      >
        {theme === 'dark' ? <Sun size={16} /> : <Moon size={16} />}
      </button>
      <LanguageSwitcher />

      {isAuthenticated ? (
        <>
          {/* Cart */}
          <Link
            to="/cart"
            className="relative p-2 rounded-full bg-white/10 border border-white/20 backdrop-blur-md hover:bg-white/20 transition-colors"
          >
            <ShoppingCart className="h-4 w-4 text-gray-700" />
            {cartItems.length > 0 && (
              <span className="absolute -top-0.5 -right-0.5 bg-red-500 text-white text-[10px] font-bold rounded-full h-4 min-w-4 flex items-center justify-center px-0.5 leading-none">
                {cartItems.length > 99 ? "99+" : cartItems.length}
              </span>
            )}
          </Link>

          {/* Avatar dropdown */}
          <div className="relative">
            <button
              onClick={() => setDropdownOpen((v) => !v)}
              className="flex items-center gap-1.5 p-1 pr-3 rounded-full bg-white/10 border border-white/20 backdrop-blur-md hover:bg-white/20 transition-colors relative z-50"
            >
              <Avatar src={user?.avatarUrl} profileType={user?.profileType} size="sm" />
              <span className="hidden sm:block text-sm font-medium text-gray-700 max-w-[90px] truncate">
                {user?.displayName}
              </span>
            </button>

            {dropdownOpen && (
              <div className="absolute right-0 mt-2 w-52 bg-white dark:bg-[#2d2a42] rounded-xl shadow-lg border border-lavender/30 py-2 z-50">
                <Link
                  to="/profile"
                  className="block px-4 py-2 text-sm text-gray-700 hover:bg-cream-dark"
                  onClick={() => setDropdownOpen(false)}
                >
                  {t("nav.profile")}
                </Link>
                <Link
                  to="/settings"
                  className="block px-4 py-2 text-sm text-gray-700 hover:bg-cream-dark"
                  onClick={() => setDropdownOpen(false)}
                >
                  {t("nav.settings")}
                </Link>
                {user?.roles.includes("Admin") && (
                  <Link
                    to="/admin"
                    className="block px-4 py-2 text-sm text-mauve font-medium hover:bg-cream-dark"
                    onClick={() => setDropdownOpen(false)}
                  >
                    {t("nav.admin")}
                  </Link>
                )}
                <hr className="my-1 border-lavender/30" />
                <button
                  onClick={() => { handleLogout(); setDropdownOpen(false); }}
                  className="w-full text-left px-4 py-2 text-sm text-red-500 hover:bg-cream-dark"
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
          <span className="hidden sm:inline">{t("nav.login") || "Sign In"}</span>
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
          headerVisible ? "translate-y-0" : "-translate-y-full"
        )}
      >
        <div className="relative flex items-start px-8 pt-6">

          {/* Logo — top left */}
          <Link
            to="/"
            className="pointer-events-auto flex items-center gap-2 bg-white/10 border border-white/20 backdrop-blur-lg rounded-full px-4 py-2 shadow-lg hover:bg-white/20 transition-colors"
          >
            <img src="/logo.png" alt="MamVibe" className="h-6 w-6 object-contain" />
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
          headerVisible ? "translate-y-0" : "-translate-y-full"
        )}
      >
        <Link to="/" className="flex items-center gap-2">
          <img src="/logo.png" alt="MamVibe" className="h-8 w-8 object-contain" />
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
      <footer className="bg-primary text-white py-10 mt-auto">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div>
              <h3 className="text-lg font-bold mb-3 flex items-center gap-2">
                <img src="/logo.png" alt="MamVibe" className="h-6 w-6 object-contain" />
                {t("footer.about")}
              </h3>
              <p className="text-peach-light text-sm">{t("footer.about_text")}</p>
            </div>
            <div>
              <h3 className="text-lg font-bold mb-3">{t("footer.links")}</h3>
              <div className="space-y-2 text-sm">
                <Link to="/browse" className="block text-peach-light hover:text-white transition-colors">
                  {t("nav.browse")}
                </Link>
                <Link to="/register" className="block text-peach-light hover:text-white transition-colors">
                  {t("nav.register")}
                </Link>
              </div>
            </div>
            <div>
              <h3 className="text-lg font-bold mb-3">{t("footer.contact")}</h3>
              <p className="text-peach-light text-sm">support@mamvibe.com</p>
            </div>
          </div>
          <div className="border-t border-white/20 mt-8 pt-6 text-center text-sm text-peach-light">
            &copy; {new Date().getFullYear()} MamVibe. {t("footer.rights")}
          </div>
        </div>
      </footer>

      <CookieConsent />
      <ScrollToTop />
    </div>
  );
}
