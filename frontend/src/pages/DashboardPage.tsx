import { useTranslation } from 'react-i18next';
import { useDashboard, type DashboardTab } from '../hooks/useDashboard';
import { itemsApi } from '../api/itemsApi';
import ItemCard from '../components/items/ItemCard';
import LoadingSpinner from '../components/common/LoadingSpinner';

export default function DashboardPage() {
  const { t } = useTranslation();
  const { tab, setTab, myItems, likedItems, payments, loading, removeLikedItem, refreshTab } = useDashboard();

  const handleListingLikeToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      refreshTab();
    } catch { /* ignore */ }
  };

  const handleLikedTabToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      removeLikedItem(id);
    } catch { /* ignore */ }
  };

  const tabs: { key: DashboardTab; label: string }[] = [
    { key: 'listings', label: t('dashboard.my_listings') },
    { key: 'liked', label: t('dashboard.liked_items') },
    { key: 'purchases', label: t('dashboard.purchases') },
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 animate-fade-in">
      <h1 className="text-3xl font-bold text-primary-dark mb-6">{t('dashboard.title')}</h1>

      <div className="flex gap-1 bg-white rounded-lg p-1 border border-lavender/30 mb-8 w-fit">
        {tabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setTab(t.key)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-all duration-300 ${
              tab === t.key ? 'bg-primary text-white shadow-md' : 'text-gray-500 hover:text-primary-dark hover:bg-lavender/20'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : (
        <>
          {tab === 'listings' && (
            myItems.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_listings')}</p>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {myItems.map((item) => <ItemCard key={item.id} item={item} onLikeToggle={handleListingLikeToggle} />)}
              </div>
            )
          )}
          {tab === 'liked' && (
            likedItems.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_liked')}</p>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {likedItems.map((item) => <ItemCard key={item.id} item={item} onLikeToggle={handleLikedTabToggle} />)}
              </div>
            )
          )}
          {tab === 'purchases' && (
            payments.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_purchases')}</p>
            ) : (
              <div className="space-y-3">
                {payments.map((p) => (
                  <div key={p.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center justify-between hover:shadow-md transition-shadow duration-300">
                    <div>
                      <p className="font-medium text-primary">{p.itemTitle}</p>
                      <p className="text-sm text-gray-400">{new Date(p.createdAt).toLocaleDateString()}</p>
                    </div>
                    <div className="text-right">
                      <p className="font-bold text-mauve">${p.amount.toFixed(2)}</p>
                      <p className="text-xs text-gray-400 capitalize">{p.paymentMethod === 0 ? 'Card' : 'On Spot'}</p>
                    </div>
                  </div>
                ))}
              </div>
            )
          )}
        </>
      )}
    </div>
  );
}
