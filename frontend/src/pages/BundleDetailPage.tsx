import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import { bundlesApi } from '../api/bundlesApi';
import { useAuthStore } from '../store/authStore';
import type { BundleDto } from '../types/bundle';
import { formatPrice } from '../utils/currency';
import Avatar from '../components/common/Avatar';
import LoadingSpinner from '../components/common/LoadingSpinner';

export default function BundleDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const [bundle, setBundle] = useState<BundleDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [requesting, setRequesting] = useState(false);

  useEffect(() => {
    if (!id) return;
    bundlesApi.getById(id)
      .then((res) => setBundle(res.data))
      .catch(() => { toast.error('Bundle not found'); navigate('/browse'); })
      .finally(() => setLoading(false));
  }, [id, navigate]);

  const handleRequest = async () => {
    if (!bundle) return;
    if (!user) { navigate('/login'); return; }
    setRequesting(true);
    try {
      await bundlesApi.requestPurchase(bundle.id);
      toast.success(t('bundle.request_success'));
      navigate('/dashboard');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error ?? t('bundle.request_error');
      toast.error(msg);
    } finally {
      setRequesting(false);
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!bundle) return null;

  const isOwner = user?.id === bundle.sellerId;

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-lavender/30 p-6 space-y-6">
        {/* Header */}
        <div className="flex items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-primary">{bundle.title}</h1>
            {bundle.description && (
              <p className="text-sm text-gray-500 mt-1">{bundle.description}</p>
            )}
          </div>
          <div className="text-right flex-shrink-0">
            <p className="text-2xl font-bold text-mauve">{formatPrice(bundle.price)}</p>
            {bundle.isSold && (
              <span className="inline-block mt-1 px-2 py-0.5 rounded-full bg-red-100 text-red-700 text-xs font-medium">
                {t('bundle.sold_badge')}
              </span>
            )}
            {!bundle.isActive && !bundle.isSold && (
              <span className="inline-block mt-1 px-2 py-0.5 rounded-full bg-gray-100 text-gray-500 text-xs font-medium">
                {t('bundle.inactive_badge')}
              </span>
            )}
          </div>
        </div>

        {/* Seller */}
        <div className="flex items-center gap-3 pt-2 border-t border-lavender/20">
          <Avatar src={bundle.sellerAvatarUrl} size="sm" />
          <div>
            <p className="text-xs text-gray-400">{t('bundle.seller_label')}</p>
            <Link to={`/profile/${bundle.sellerId}`} className="text-sm font-medium text-primary hover:underline">
              {bundle.sellerDisplayName ?? 'Unknown'}
            </Link>
          </div>
        </div>

        {/* Items */}
        <div>
          <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">
            {t('bundle.items_label')} ({bundle.items.length})
          </h2>
          <div className="space-y-2">
            {bundle.items.map((item) => (
              <div key={item.itemId} className="flex items-center gap-3 p-2 rounded-lg border border-lavender/20">
                {item.photoUrl ? (
                  <img src={item.photoUrl} alt={item.title} className="w-12 h-12 rounded-lg object-cover flex-shrink-0" />
                ) : (
                  <div className="w-12 h-12 rounded-lg bg-lavender/20 flex-shrink-0" />
                )}
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-primary truncate">{item.title}</p>
                  <p className="text-xs text-gray-400">{item.price != null ? formatPrice(item.price) : t('items.free')}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Action */}
        {!isOwner && bundle.isActive && !bundle.isSold && (
          <button
            onClick={handleRequest}
            disabled={requesting}
            className="w-full py-3 rounded-xl bg-mauve text-white font-semibold hover:bg-mauve/90 disabled:opacity-60 transition-colors"
          >
            {requesting ? '…' : t('bundle.request_btn')}
          </button>
        )}
      </div>
    </div>
  );
}
