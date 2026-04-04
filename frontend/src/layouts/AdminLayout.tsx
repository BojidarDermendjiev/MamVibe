import { Link, Outlet, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiChartBar, HiUsers, HiCollection, HiArrowLeft, HiTruck } from 'react-icons/hi';
import { Sun, Moon, Wallet } from 'lucide-react';
import { clsx } from 'clsx';
import { useTheme } from '../contexts/ThemeContext';

const navItems = [
  { path: '/admin', icon: HiChartBar, labelKey: 'admin.dashboard', exact: true },
  { path: '/admin/users', icon: HiUsers, labelKey: 'admin.users', exact: false },
  { path: '/admin/items', icon: HiCollection, labelKey: 'admin.items', exact: false },
  { path: '/admin/shipping', icon: HiTruck, labelKey: 'shipping.admin_title', exact: false },
  { path: '/admin/wallets', icon: Wallet, labelKey: 'wallet.admin_title', exact: false },
];

export default function AdminLayout() {
  const { t } = useTranslation();
  const location = useLocation();
  const { theme, toggleTheme } = useTheme();

  const isActive = (path: string, exact: boolean) =>
    exact ? location.pathname === path : location.pathname.startsWith(path);

  return (
    <div className="min-h-screen bg-white dark:bg-[#1a1825] flex transition-colors duration-300">

      {/* ── Desktop Sidebar ── */}
      <aside className="w-64 bg-primary dark:bg-[#2d2a42] text-white flex-shrink-0 hidden md:flex flex-col transition-colors duration-300">

        {/* Sidebar header with logo + theme toggle */}
        <div className="p-6 flex items-center justify-between">
          <h2 className="text-xl font-bold flex items-center gap-2">
            <img src="/logo.png" alt="MamVibe" className="h-7 w-7 object-contain" />
            MamVibe Admin
          </h2>
          <button
            onClick={toggleTheme}
            title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            aria-label={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            className="p-2 rounded-lg bg-white/10 hover:bg-white/20 text-white transition-colors flex-shrink-0"
          >
            {theme === 'dark' ? <Sun size={16} /> : <Moon size={16} />}
          </button>
        </div>

        <nav className="flex-1 px-4 space-y-1">
          {navItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={clsx(
                'flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors',
                isActive(item.path, item.exact)
                  ? 'bg-white/20 text-white'
                  : 'text-lavender-light hover:bg-white/10 hover:text-white'
              )}
            >
              <item.icon className="h-5 w-5" />
              {t(item.labelKey)}
            </Link>
          ))}
        </nav>

        <div className="p-4">
          <Link
            to="/"
            className="flex items-center gap-2 px-4 py-3 rounded-lg text-sm text-lavender-light hover:bg-white/10 hover:text-white transition-colors"
          >
            <HiArrowLeft className="h-5 w-5" />
            {t('common.back')}
          </Link>
        </div>
      </aside>

      {/* ── Mobile header ── */}
      <div className="md:hidden fixed top-0 left-0 right-0 bg-primary dark:bg-[#2d2a42] text-white p-4 z-40 flex items-center gap-4 transition-colors duration-300">
        <Link to="/" className="text-lavender-light hover:text-white">
          <HiArrowLeft className="h-5 w-5" />
        </Link>
        <h2 className="font-bold">Admin Panel</h2>
        <div className="flex gap-2 ml-auto items-center">
          {navItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              aria-label={t(item.labelKey)}
              title={t(item.labelKey)}
              className={clsx(
                'p-2 rounded-lg transition-colors',
                isActive(item.path, item.exact)
                  ? 'bg-white/20'
                  : 'text-lavender-light hover:bg-white/10'
              )}
            >
              <item.icon className="h-5 w-5" />
            </Link>
          ))}
          <button
            onClick={toggleTheme}
            title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            aria-label={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            className="p-2 rounded-lg bg-white/10 hover:bg-white/20 text-white transition-colors"
          >
            {theme === 'dark' ? <Sun size={15} /> : <Moon size={15} />}
          </button>
        </div>
      </div>

      {/* ── Content ── */}
      <div className="flex-1 md:p-8 p-4 md:pt-8 pt-20 animate-fade-in">
        <Outlet />
      </div>
    </div>
  );
}
