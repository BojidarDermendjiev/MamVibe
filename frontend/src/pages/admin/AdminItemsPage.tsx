import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { itemsApi } from '../../api/itemsApi';
import { adminApi } from '../../api/adminApi';
import { type Item, ListingType } from '../../types/item';
import Button from '../../components/common/Button';
import LoadingSpinner from '../../components/common/LoadingSpinner';

export default function AdminItemsPage() {
  const { t } = useTranslation();
  const [items, setItems] = useState<Item[]>([]);
  const [pendingItems, setPendingItems] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      itemsApi.getAll({ page: 1, pageSize: 50, sortBy: 'newest' }),
      adminApi.getPendingItems(),
    ]).then(([itemsRes, pendingRes]) => {
      setItems(itemsRes.data.items);
      setPendingItems(pendingRes.data);
      setLoading(false);
    }).catch(() => setLoading(false));
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this item?')) return;
    try {
      await adminApi.deleteItem(id);
      setItems((prev) => prev.filter((i) => i.id !== id));
      setPendingItems((prev) => prev.filter((i) => i.id !== id));
      toast.success('Item deleted');
    } catch {
      toast.error(t('common.error'));
    }
  };

  const handleApprove = async (id: string) => {
    try {
      await adminApi.approveItem(id);
      const approved = pendingItems.find((i) => i.id === id);
      setPendingItems((prev) => prev.filter((i) => i.id !== id));
      if (approved) setItems((prev) => [approved, ...prev]);
      toast.success(t('admin.approve_item'));
    } catch {
      toast.error(t('common.error'));
    }
  };

  return (
    <div>
      <h1 className="text-3xl font-bold text-primary mb-6">{t('admin.items')}</h1>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : (
        <>
          {/* Pending Approval Section */}
          <div className="mb-8">
            <h2 className="text-xl font-semibold text-primary mb-4 flex items-center gap-2">
              {t('admin.pending_items')}
              {pendingItems.length > 0 && (
                <span className="bg-amber-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
                  {pendingItems.length}
                </span>
              )}
            </h2>
            {pendingItems.length === 0 ? (
              <p className="text-gray-500 bg-white rounded-xl p-4 border border-lavender/30">
                {t('admin.no_pending')}
              </p>
            ) : (
              <div className="bg-white rounded-xl border border-amber-200 overflow-hidden">
                <table className="w-full">
                  <thead>
                    <tr className="bg-amber-50 text-left">
                      <th className="px-4 py-3 text-sm font-medium text-primary">Title</th>
                      <th className="px-4 py-3 text-sm font-medium text-primary">Type</th>
                      <th className="px-4 py-3 text-sm font-medium text-primary">Price</th>
                      <th className="px-4 py-3 text-sm font-medium text-primary">Owner</th>
                      <th className="px-4 py-3 text-sm font-medium text-primary">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-amber-100">
                    {pendingItems.map((item) => (
                      <tr key={item.id} className="hover:bg-amber-50/50">
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-3">
                            {item.photos?.[0] && (
                              <img
                                src={item.photos[0].url}
                                alt={item.title}
                                className="w-10 h-10 rounded-lg object-cover"
                              />
                            )}
                            <span className="text-sm font-medium text-primary">{item.title}</span>
                          </div>
                        </td>
                        <td className="px-4 py-3">
                          <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                            item.listingType === ListingType.Donate
                              ? 'bg-green-100 text-green-600'
                              : 'bg-mauve/10 text-mauve'
                          }`}>
                            {item.listingType === ListingType.Donate ? 'Donate' : 'Sell'}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-sm text-gray-500">
                          {item.price ? `${item.price.toFixed(2)} BGN` : 'Free'}
                        </td>
                        <td className="px-4 py-3 text-sm text-gray-500">{item.userDisplayName}</td>
                        <td className="px-4 py-3 flex gap-2">
                          <Button size="sm" onClick={() => handleApprove(item.id)}>
                            {t('admin.approve_item')}
                          </Button>
                          <Button size="sm" variant="danger" onClick={() => handleDelete(item.id)}>
                            {t('admin.delete_item')}
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {/* Active Items Table */}
          <div className="bg-white rounded-xl border border-lavender/30 overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="bg-cream-dark text-left">
                  <th className="px-4 py-3 text-sm font-medium text-primary">Title</th>
                  <th className="px-4 py-3 text-sm font-medium text-primary">Type</th>
                  <th className="px-4 py-3 text-sm font-medium text-primary">Price</th>
                  <th className="px-4 py-3 text-sm font-medium text-primary">Owner</th>
                  <th className="px-4 py-3 text-sm font-medium text-primary">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-lavender/20">
                {items.map((item) => (
                  <tr key={item.id} className="hover:bg-cream/50">
                    <td className="px-4 py-3">
                      <span className="text-sm font-medium text-primary">{item.title}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                        item.listingType === ListingType.Donate
                          ? 'bg-green-100 text-green-600'
                          : 'bg-mauve/10 text-mauve'
                      }`}>
                        {item.listingType === ListingType.Donate ? 'Donate' : 'Sell'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-500">
                      {item.price ? `${item.price.toFixed(2)} BGN` : 'Free'}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-500">{item.userDisplayName}</td>
                    <td className="px-4 py-3">
                      <Button size="sm" variant="danger" onClick={() => handleDelete(item.id)}>
                        {t('admin.delete_item')}
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
