import { Link, Outlet, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiChartBar, HiUsers, HiCollection, HiArrowLeft, HiTruck } from 'react-icons/hi';
import { clsx } from 'clsx';

const navItems = [
  { path: '/admin', icon: HiChartBar, labelKey: 'admin.dashboard', exact: true },
  { path: '/admin/users', icon: HiUsers, labelKey: 'admin.users', exact: false },
  { path: '/admin/items', icon: HiCollection, labelKey: 'admin.items', exact: false },
  { path: '/admin/shipping', icon: HiTruck, labelKey: 'shipping.admin_title', exact: false },
];

export default function AdminLayout() {
  const { t } = useTranslation();
  const location = useLocation();

  const isActive = (path: string, exact: boolean) =>
    exact ? location.pathname === path : location.pathname.startsWith(path);

  return (
    <div className="min-h-screen bg-peach flex">
      {/* Sidebar */}
      <aside className="w-64 bg-primary text-white flex-shrink-0 hidden md:flex flex-col">
        <div className="p-6">
          <h2 className="text-xl font-bold flex items-center gap-2">
            <img src="/logo.png" alt="MamVibe" className="h-7 w-7 object-contain" /> MamVibe Admin
          </h2>
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

      {/* Mobile header */}
      <div className="md:hidden fixed top-0 left-0 right-0 bg-primary text-white p-4 z-40 flex items-center gap-4">
        <Link to="/" className="text-lavender-light hover:text-white">
          <HiArrowLeft className="h-5 w-5" />
        </Link>
        <h2 className="font-bold">Admin Panel</h2>
        <div className="flex gap-2 ml-auto">
          {navItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
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
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 md:p-8 p-4 md:pt-8 pt-20 animate-fade-in">
        <Outlet />
      </div>
    </div>
  );
}
