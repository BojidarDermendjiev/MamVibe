import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HiUsers, HiCollection, HiGift, HiCurrencyDollar } from 'react-icons/hi';
import { adminApi, type DashboardStats } from '../../api/adminApi';
import LoadingSpinner from '../../components/common/LoadingSpinner';

export default function AdminDashboardPage() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminApi.getDashboard().then((res) => {
      setStats(res.data);
      setLoading(false);
    }).catch(() => setLoading(false));
  }, []);

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!stats) return null;

  const cards = [
    { label: t('admin.total_users'), value: stats.totalUsers, icon: HiUsers, color: 'bg-lavender/20 text-primary' },
    { label: t('admin.total_items'), value: stats.totalItems, icon: HiCollection, color: 'bg-peach/20 text-mauve' },
    { label: t('admin.total_donations'), value: stats.totalDonations, icon: HiGift, color: 'bg-green-100 text-green-600' },
    { label: t('admin.total_sales'), value: `$${stats.totalRevenue.toFixed(2)}`, icon: HiCurrencyDollar, color: 'bg-mauve/10 text-mauve' },
  ];

  return (
    <div>
      <h1 className="text-3xl font-bold text-primary mb-8">{t('admin.dashboard')}</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {cards.map((card, i) => (
          <div key={i} className="bg-white rounded-xl p-6 border border-lavender/30">
            <div className={`w-12 h-12 rounded-xl ${card.color} flex items-center justify-center mb-4`}>
              <card.icon className="h-6 w-6" />
            </div>
            <p className="text-sm text-gray-500">{card.label}</p>
            <p className="text-2xl font-bold text-primary mt-1">{card.value}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
