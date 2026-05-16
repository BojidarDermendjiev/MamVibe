import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HiX, HiChevronLeft, HiChevronRight, HiEye } from 'react-icons/hi';
import toast from '@/utils/toast';
import { itemsApi } from '../../api/itemsApi';
import { adminApi, type ModerationLogEntry } from '../../api/adminApi';
import { type Item, ListingType } from '../../types/item';
import Button from '../../components/common/Button';
import Avatar from '../../components/common/Avatar';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import { formatPrice } from '../../utils/currency';

function ItemDetailModal({
  item,
  isPending,
  onClose,
  onApprove,
  onDelete,
}: {
  item: Item;
  isPending: boolean;
  onClose: () => void;
  onApprove: (id: string) => void;
  onDelete: (id: string) => void;
}) {
  const { t } = useTranslation();
  const [photoIndex, setPhotoIndex] = useState(0);
  const [showFlagConfirm, setShowFlagConfirm] = useState(false);
  const [history, setHistory] = useState<ModerationLogEntry[]>([]);

  useEffect(() => {
    adminApi.getModerationHistory(item.id)
      .then((res) => setHistory(res.data))
      .catch(() => {});
  }, [item.id]);
  const photos = item.photos.slice().sort((a, b) => a.displayOrder - b.displayOrder);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4"
      onClick={onClose}
    >
      <div
        className="bg-white dark:bg-[#2d2a42] rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-lavender/20 dark:border-white/10">
          <h2 className="text-lg font-semibold text-[#364153] dark:text-[#bdb9bc] truncate pr-4">{item.title}</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 flex-shrink-0"
          >
            <HiX className="h-5 w-5" />
          </button>
        </div>

        {/* Photo gallery */}
        {photos.length > 0 && (
          <div className="relative bg-gray-100 dark:bg-black/20">
            <img
              src={photos[photoIndex].url}
              alt={item.title}
              className="w-full h-72 object-contain"
            />
            {photos.length > 1 && (
              <>
                <button
                  onClick={() => setPhotoIndex((i) => (i - 1 + photos.length) % photos.length)}
                  className="absolute left-2 top-1/2 -translate-y-1/2 bg-black/40 hover:bg-black/60 text-white rounded-full p-1"
                >
                  <HiChevronLeft className="h-5 w-5" />
                </button>
                <button
                  onClick={() => setPhotoIndex((i) => (i + 1) % photos.length)}
                  className="absolute right-2 top-1/2 -translate-y-1/2 bg-black/40 hover:bg-black/60 text-white rounded-full p-1"
                >
                  <HiChevronRight className="h-5 w-5" />
                </button>
                <div className="absolute bottom-2 left-1/2 -translate-x-1/2 flex gap-1">
                  {photos.map((_, i) => (
                    <button
                      key={i}
                      onClick={() => setPhotoIndex(i)}
                      className={`w-2 h-2 rounded-full transition-colors ${i === photoIndex ? 'bg-white' : 'bg-white/40'}`}
                    />
                  ))}
                </div>
                {/* Thumbnails */}
                <div className="flex gap-2 p-3 overflow-x-auto">
                  {photos.map((p, i) => (
                    <button key={p.id} onClick={() => setPhotoIndex(i)}>
                      <img
                        src={p.url}
                        alt=""
                        className={`h-14 w-14 rounded-lg object-cover flex-shrink-0 border-2 transition-colors ${
                          i === photoIndex ? 'border-primary' : 'border-transparent'
                        }`}
                      />
                    </button>
                  ))}
                </div>
              </>
            )}
          </div>
        )}

        {/* Details */}
        <div className="px-6 py-4 space-y-4">
          {/* Meta row */}
          <div className="flex flex-wrap gap-3 items-center">
            <span className={`px-2 py-1 rounded-full text-xs font-medium ${
              item.listingType === ListingType.Donate ? 'bg-green-100 text-green-700' : 'bg-mauve/10 text-mauve'
            }`}>
              {item.listingType === ListingType.Donate ? t('items.donate') : t('items.sell')}
            </span>
            {item.price != null && item.price > 0 && (
              <span className="text-lg font-bold text-mauve">{formatPrice(item.price)}</span>
            )}
            {(item.price == null || item.price === 0) && item.listingType === ListingType.Donate && (
              <span className="text-lg font-bold text-green-600">{t('items.free')}</span>
            )}
            {item.categoryName && (
              <span className="text-xs text-gray-500 dark:text-gray-400 bg-gray-100 dark:bg-white/10 px-2 py-1 rounded-full">
                {item.categoryName}
              </span>
            )}
            <span className="text-xs text-gray-400 ml-auto">
              {new Date(item.createdAt).toLocaleDateString()}
            </span>
          </div>

          {/* AI Moderation */}
          {item.aiModerationStatus !== 0 && (
            <div className={`p-3 rounded-lg text-sm border ${
              item.aiModerationStatus === 3
                ? 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-900/40'
                : item.aiModerationStatus === 2
                ? 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-200 dark:border-yellow-900/40'
                : 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-900/40'
            }`}>
              <div className="flex items-center gap-2 flex-wrap">
                <span className={`font-semibold ${
                  item.aiModerationStatus === 3 ? 'text-red-700 dark:text-red-400' :
                  item.aiModerationStatus === 2 ? 'text-yellow-700 dark:text-yellow-400' :
                  'text-green-700 dark:text-green-400'
                }`}>
                  {item.aiModerationStatus === 3 ? 'AI: Flagged' :
                   item.aiModerationStatus === 2 ? 'AI: Needs Review' : 'AI: Auto-Approved'}
                </span>
                {item.aiModerationScore != null && (
                  <span className="text-xs text-gray-500 dark:text-gray-400">
                    ({Math.round(item.aiModerationScore * 100)}% confidence)
                  </span>
                )}
              </div>
              {item.aiModerationNotes && (
                <p className="mt-1 text-xs text-gray-600 dark:text-gray-300">{item.aiModerationNotes}</p>
              )}
            </div>
          )}

          {/* Description */}
          {item.description && (
            <p className="text-sm text-gray-600 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">
              {item.description}
            </p>
          )}

          {/* Owner */}
          <div className="flex items-center gap-2 pt-1 border-t border-lavender/20 dark:border-white/10">
            <Avatar src={item.userAvatarUrl} size="sm" />
            <span className="text-sm text-gray-600 dark:text-gray-300">{item.userDisplayName}</span>
          </div>

          {/* Moderation history */}
          {history.length > 0 && (
            <div className="pt-2 border-t border-lavender/20 dark:border-white/10">
              <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">Moderation history</p>
              <ul className="space-y-1.5">
                {history.map((entry, i) => (
                  <li key={i} className="flex items-start gap-2 text-xs text-gray-600 dark:text-gray-300">
                    <span className={`mt-0.5 w-2 h-2 rounded-full flex-shrink-0 ${entry.action === 'Approved' ? 'bg-green-500' : 'bg-red-500'}`} />
                    <span>
                      <span className="font-medium">{entry.adminDisplayName}</span>
                      {' '}{entry.action === 'Approved' ? 'approved' : 'deleted'} this item
                      {' '}·{' '}
                      <span className="text-gray-400">{new Date(entry.timestamp).toLocaleString()}</span>
                      {' '}·{' '}
                      <span className="text-gray-400">AI was: {entry.aiStatusAtTime.replace(/([A-Z])/g, ' $1').trim()}</span>
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>

        {/* Actions */}
        <div className="flex gap-3 px-6 py-4 border-t border-lavender/20 dark:border-white/10">
          {isPending && (
            <Button
              size="sm"
              onClick={() => {
                if (item.aiModerationStatus === 3) {
                  setShowFlagConfirm(true);
                } else {
                  onApprove(item.id);
                  onClose();
                }
              }}
            >
              {t('admin.approve_item')}
            </Button>
          )}
          <Button
            size="sm"
            variant="danger"
            onClick={() => { onDelete(item.id); onClose(); }}
          >
            {t('admin.delete_item')}
          </Button>
          <button
            onClick={onClose}
            className="ml-auto text-sm text-gray-500 hover:text-gray-700 dark:hover:text-gray-300"
          >
            {t('common.cancel')}
          </button>
        </div>
      </div>

      {/* Flagged-item confirmation overlay */}
      {showFlagConfirm && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/70 p-4">
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl shadow-2xl w-full max-w-md p-6 space-y-4">
            <div className="flex items-start gap-3">
              <span className="text-2xl">⚠️</span>
              <div>
                <h3 className="font-bold text-red-600 dark:text-red-400 text-lg">AI flagged this item</h3>
                <p className="text-sm text-gray-600 dark:text-gray-300 mt-1">
                  The AI moderation system marked this listing as potentially inappropriate.
                </p>
                {item.aiModerationNotes && (
                  <p className="mt-2 text-xs bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-300 rounded-lg px-3 py-2 border border-red-200 dark:border-red-900/40">
                    {item.aiModerationNotes}
                  </p>
                )}
                <p className="mt-3 text-sm font-medium text-gray-700 dark:text-gray-200">
                  Are you sure you want to approve it anyway?
                </p>
              </div>
            </div>
            <div className="flex gap-3 justify-end pt-2">
              <button
                onClick={() => setShowFlagConfirm(false)}
                className="px-4 py-2 text-sm text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-white"
              >
                Cancel
              </button>
              <Button
                size="sm"
                variant="danger"
                onClick={() => { onApprove(item.id); onClose(); }}
              >
                Approve anyway
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default function AdminItemsPage() {
  const { t } = useTranslation();
  const [items, setItems] = useState<Item[]>([]);
  const [pendingItems, setPendingItems] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedItem, setSelectedItem] = useState<{ item: Item; isPending: boolean } | null>(null);

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
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-6">{t('admin.items')}</h1>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : (
        <>
          {/* Pending Approval Section */}
          <div className="mb-8">
            <h2 className="text-xl font-semibold text-[#364153] dark:text-[#bdb9bc] mb-4 flex items-center gap-2">
              {t('admin.pending_items')}
              {pendingItems.length > 0 && (
                <span className="bg-amber-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
                  {pendingItems.length}
                </span>
              )}
            </h2>
            {pendingItems.length === 0 ? (
              <p className="text-gray-500 dark:text-gray-400 bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 dark:border-white/10">
                {t('admin.no_pending')}
              </p>
            ) : (
              <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-amber-200 dark:border-amber-900/40 overflow-hidden">
                <table className="w-full">
                  <thead>
                    <tr className="bg-amber-50 dark:bg-amber-900/20 text-left">
                      <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Title</th>
                      <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Type</th>
                      <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Price</th>
                      <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">AI Screen</th>
                      <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Owner</th>
                      <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-amber-100 dark:divide-amber-900/30">
                    {pendingItems.map((item) => (
                      <tr
                        key={item.id}
                        className="hover:bg-amber-50/50 dark:hover:bg-amber-900/10 cursor-pointer"
                        onClick={() => setSelectedItem({ item, isPending: true })}
                      >
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-3">
                            {item.photos?.[0] ? (
                              <img
                                src={item.photos[0].url}
                                alt={item.title}
                                className="w-10 h-10 rounded-lg object-cover flex-shrink-0"
                              />
                            ) : (
                              <div className="w-10 h-10 rounded-lg bg-lavender/20 flex-shrink-0" />
                            )}
                            <span className="text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">{item.title}</span>
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
                        <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">
                          {item.price ? formatPrice(item.price) : 'Free'}
                        </td>
                        <td className="px-4 py-3">
                          {item.aiModerationStatus === 3 ? (
                            <span className="px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400">
                              Flagged
                            </span>
                          ) : item.aiModerationStatus === 2 ? (
                            <span className="px-2 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400">
                              Review
                            </span>
                          ) : (
                            <span className="px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-500 dark:bg-white/10 dark:text-gray-400">
                              —
                            </span>
                          )}
                        </td>
                        <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{item.userDisplayName}</td>
                        <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
                          <div className="flex gap-2">
                            <button
                              onClick={() => setSelectedItem({ item, isPending: true })}
                              className="p-1.5 text-gray-400 hover:text-primary transition-colors"
                              title="View details"
                            >
                              <HiEye className="h-4 w-4" />
                            </button>
                            {item.aiModerationStatus !== 3 && (
                              <Button size="sm" onClick={() => handleApprove(item.id)}>
                                {t('admin.approve_item')}
                              </Button>
                            )}
                            <Button size="sm" variant="danger" onClick={() => handleDelete(item.id)}>
                              {t('admin.delete_item')}
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {/* Active Items Table */}
          <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                  <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Title</th>
                  <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Type</th>
                  <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Price</th>
                  <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Owner</th>
                  <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
                {items.map((item) => (
                  <tr
                    key={item.id}
                    className="hover:bg-cream/50 dark:hover:bg-white/5 cursor-pointer"
                    onClick={() => setSelectedItem({ item, isPending: false })}
                  >
                    <td className="px-4 py-3">
                      <span className="text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">{item.title}</span>
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
                    <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">
                      {item.price ? `${item.price.toFixed(2)} BGN` : 'Free'}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{item.userDisplayName}</td>
                    <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
                      <div className="flex gap-2">
                        <button
                          onClick={() => setSelectedItem({ item, isPending: false })}
                          className="p-1.5 text-gray-400 hover:text-primary transition-colors"
                          title="View details"
                        >
                          <HiEye className="h-4 w-4" />
                        </button>
                        <Button size="sm" variant="danger" onClick={() => handleDelete(item.id)}>
                          {t('admin.delete_item')}
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {selectedItem && (
        <ItemDetailModal
          item={selectedItem.item}
          isPending={selectedItem.isPending}
          onClose={() => setSelectedItem(null)}
          onApprove={handleApprove}
          onDelete={handleDelete}
        />
      )}
    </div>
  );
}
