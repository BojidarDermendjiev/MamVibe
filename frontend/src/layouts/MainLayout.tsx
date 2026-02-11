import { useState } from "react";
import { Link, Outlet, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { HiMenu, HiX, HiChat, HiPlusCircle, HiShoppingCart } from "react-icons/hi";
import { useAuthStore } from "../store/authStore";
import { useCartStore } from "../store/cartStore";
import { useNotification } from "../contexts/NotificationContext";
import LanguageSwitcher from "../components/common/LanguageSwitcher";
import Avatar from "../components/common/Avatar";
import CookieConsent from "../components/common/CookieConsent";

export default function MainLayout() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user, isAuthenticated, logout } = useAuthStore();
  const { unreadCount } = useNotification();
  const cartItems = useCartStore((s) => s.items);
  const [menuOpen, setMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  return (
    <div className="min-h-screen flex flex-col bg-peach">
      {/* Navbar */}
      <nav className="bg-peach/80 backdrop-blur-md border-b border-lavender/30 sticky top-0 z-40 transition-colors duration-300">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <Link to="/" className="flex items-center gap-2">
              <img
                src="/logo.png"
                alt="MomVibe"
                className="h-24 w-24 object-contain"
              />
              <span className="text-xl font-bold text-primary">MomVibe</span>
            </Link>

            {/* Right side */}
            <div className="hidden md:flex items-center gap-3">
              <LanguageSwitcher />
              {isAuthenticated ? (
                <>
                  {/* Cart icon */}
                  <Link
                    to="/cart"
                    className="relative p-2 rounded-full hover:bg-cream-dark transition-colors"
                  >
                    <HiShoppingCart className="h-6 w-6 text-primary" />
                    {cartItems.length > 0 && (
                      <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs font-bold rounded-full h-5 min-w-5 flex items-center justify-center px-1">
                        {cartItems.length > 99 ? "99+" : cartItems.length}
                      </span>
                    )}
                  </Link>
                  {/* User menu */}
                  <div className="relative group">
                    <button className="flex items-center gap-2 p-1 rounded-full hover:bg-cream-dark transition-colors">
                      <Avatar
                        src={user?.avatarUrl}
                        profileType={user?.profileType}
                        size="sm"
                      />
                      <span className="text-sm font-medium text-primary">
                        {user?.displayName}
                      </span>
                    </button>
                    <div className="absolute right-0 mt-1 w-56 bg-white rounded-xl shadow-lg border border-lavender/30 py-2 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200">
                      <Link
                        to="/create"
                        className="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-cream-dark"
                      >
                        <HiPlusCircle className="h-4 w-4" />
                        {t("nav.create")}
                      </Link>
                      <Link
                        to="/chat"
                        className="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-cream-dark"
                      >
                        <HiChat className="h-4 w-4" />
                        {t("nav.chat")}
                        {unreadCount > 0 && (
                          <span className="ml-auto bg-red-500 text-white text-xs font-bold rounded-full h-5 min-w-5 flex items-center justify-center px-1">
                            {unreadCount > 99 ? "99+" : unreadCount}
                          </span>
                        )}
                      </Link>
                      <Link
                        to="/dashboard"
                        className="block px-4 py-2 text-sm text-gray-700 hover:bg-cream-dark"
                      >
                        {t("nav.dashboard")}
                      </Link>
                      <Link
                        to="/feedback"
                        className="block px-4 py-2 text-sm text-gray-700 hover:bg-cream-dark"
                      >
                        {t("nav.feedback")}
                      </Link>
                      <hr className="my-1 border-lavender/30" />
                      <Link
                        to="/profile"
                        className="block px-4 py-2 text-sm text-gray-700 hover:bg-cream-dark"
                      >
                        {t("nav.profile")}
                      </Link>
                      <Link
                        to="/settings"
                        className="block px-4 py-2 text-sm text-gray-700 hover:bg-cream-dark"
                      >
                        {t("nav.settings")}
                      </Link>
                      {user?.roles.includes("Admin") && (
                        <Link
                          to="/admin"
                          className="block px-4 py-2 text-sm text-mauve font-medium hover:bg-cream-dark"
                        >
                          {t("nav.admin")}
                        </Link>
                      )}
                      <hr className="my-1 border-lavender/30" />
                      <button
                        onClick={handleLogout}
                        className="w-full text-left px-4 py-2 text-sm text-red-500 hover:bg-cream-dark"
                      >
                        {t("nav.logout")}
                      </button>
                    </div>
                  </div>
                </>
              ) : (
                <div className="flex items-center gap-2">
                  <Link
                    to="/login"
                    className="px-4 py-2 text-sm font-medium text-primary hover:text-primary-dark transition-colors"
                  >
                    {t("nav.login")}
                  </Link>
                  <Link
                    to="/register"
                    className="px-4 py-2 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary-dark hover:shadow-lg hover:shadow-primary-dark/30 transition-all duration-300"
                  >
                    {t("nav.register")}
                  </Link>
                </div>
              )}
            </div>

            {/* Mobile menu button */}
            <button
              onClick={() => setMenuOpen(!menuOpen)}
              className="md:hidden p-2 rounded-lg hover:bg-cream-dark"
            >
              {menuOpen ? (
                <HiX className="h-6 w-6" />
              ) : (
                <HiMenu className="h-6 w-6" />
              )}
            </button>
          </div>
        </div>

        {/* Mobile menu */}
        {menuOpen && (
          <div className="md:hidden bg-white border-t border-lavender/30 py-4 px-4 space-y-3">
            <Link
              to="/browse"
              onClick={() => setMenuOpen(false)}
              className="block py-2 text-text font-medium"
            >
              {t("nav.browse")}
            </Link>
            {isAuthenticated ? (
              <>
                <Link
                  to="/cart"
                  onClick={() => setMenuOpen(false)}
                  className="flex items-center gap-2 py-2 text-text font-medium"
                >
                  <HiShoppingCart className="h-5 w-5" />
                  {t("nav.cart")}
                  {cartItems.length > 0 && (
                    <span className="bg-red-500 text-white text-xs font-bold rounded-full h-5 min-w-5 flex items-center justify-center px-1">
                      {cartItems.length}
                    </span>
                  )}
                </Link>
                <Link
                  to="/create"
                  onClick={() => setMenuOpen(false)}
                  className="block py-2 text-text font-medium"
                >
                  {t("nav.create")}
                </Link>
                <Link
                  to="/chat"
                  onClick={() => setMenuOpen(false)}
                  className="flex items-center gap-2 py-2 text-text font-medium"
                >
                  {t("nav.chat")}
                  {unreadCount > 0 && (
                    <span className="bg-red-500 text-white text-xs font-bold rounded-full h-5 min-w-5 flex items-center justify-center px-1">
                      {unreadCount > 99 ? "99+" : unreadCount}
                    </span>
                  )}
                </Link>
                <Link
                  to="/dashboard"
                  onClick={() => setMenuOpen(false)}
                  className="block py-2 text-text font-medium"
                >
                  {t("nav.dashboard")}
                </Link>
                <Link
                  to="/profile"
                  onClick={() => setMenuOpen(false)}
                  className="block py-2 text-text font-medium"
                >
                  {t("nav.profile")}
                </Link>
                <Link
                  to="/settings"
                  onClick={() => setMenuOpen(false)}
                  className="block py-2 text-text font-medium"
                >
                  {t("nav.settings")}
                </Link>
                <Link
                  to="/feedback"
                  onClick={() => setMenuOpen(false)}
                  className="block py-2 text-text font-medium"
                >
                  {t("nav.feedback")}
                </Link>
                {user?.roles.includes("Admin") && (
                  <Link
                    to="/admin"
                    onClick={() => setMenuOpen(false)}
                    className="block py-2 text-mauve font-medium"
                  >
                    {t("nav.admin")}
                  </Link>
                )}
                <button
                  onClick={() => {
                    handleLogout();
                    setMenuOpen(false);
                  }}
                  className="block py-2 text-red-500 font-medium"
                >
                  {t("nav.logout")}
                </button>
              </>
            ) : (
              <>
                <Link
                  to="/login"
                  onClick={() => setMenuOpen(false)}
                  className="block py-2 text-primary font-medium"
                >
                  {t("nav.login")}
                </Link>
                <Link
                  to="/register"
                  onClick={() => setMenuOpen(false)}
                  className="block py-2 text-primary font-medium"
                >
                  {t("nav.register")}
                </Link>
              </>
            )}
            <div className="pt-2">
              <LanguageSwitcher />
            </div>
          </div>
        )}
      </nav>

      {/* Main content */}
      <main className="flex-1 animate-fade-in">
        <Outlet />
      </main>

      {/* Footer */}
      <footer className="bg-primary text-white py-10 mt-auto">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div>
              <h3 className="text-lg font-bold mb-3 flex items-center gap-2">
                <img
                  src="/logo.png"
                  alt="MomVibe"
                  className="h-6 w-6 object-contain"
                />{" "}
                {t("footer.about")}
              </h3>
              <p className="text-peach-light text-sm">
                {t("footer.about_text")}
              </p>
            </div>
            <div>
              <h3 className="text-lg font-bold mb-3">{t("footer.links")}</h3>
              <div className="space-y-2 text-sm">
                <Link
                  to="/browse"
                  className="block text-peach-light hover:text-white transition-colors"
                >
                  {t("nav.browse")}
                </Link>
                <Link
                  to="/register"
                  className="block text-peach-light hover:text-white transition-colors"
                >
                  {t("nav.register")}
                </Link>
              </div>
            </div>
            <div>
              <h3 className="text-lg font-bold mb-3">{t("footer.contact")}</h3>
              <p className="text-peach-light text-sm">support@momvibe.com</p>
            </div>
          </div>
          <div className="border-t border-white/20 mt-8 pt-6 text-center text-sm text-peach-light">
            &copy; {new Date().getFullYear()} MomVibe. {t("footer.rights")}
          </div>
        </div>
      </footer>

      <CookieConsent />
    </div>
  );
}
