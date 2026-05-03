import { useState } from 'react';
import { X } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import { userRatingsApi } from '../../api/userRatingsApi';
import StarRating from '../common/StarRating';

interface RateSellerModalProps {
  purchaseRequestId: string;
  sellerName: string | null;
  onClose: () => void;
  onRated: () => void;
}

export default function RateSellerModal({ purchaseRequestId, sellerName, onClose, onRated }: RateSellerModalProps) {
  const { t } = useTranslation();
  const [rating, setRating] = useState(0);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async () => {
    if (rating === 0) {
      toast.error(t('rating.select_stars'));
      return;
    }
    setSubmitting(true);
    try {
      await userRatingsApi.create(purchaseRequestId, { rating, comment: comment.trim() || undefined });
      toast.success(t('rating.submitted'));
      onRated();
      onClose();
    } catch {
      toast.error(t('common.error'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-fade-in">
      <div className="w-full max-w-sm bg-white dark:bg-gray-800 rounded-2xl shadow-2xl overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-lavender/20">
          <h2 className="font-semibold text-gray-900 dark:text-gray-100">{t('rating.rate_seller')}</h2>
          <button onClick={onClose} className="p-1 rounded-full text-gray-400 hover:text-gray-600 transition-colors">
            <X size={16} />
          </button>
        </div>

        <div className="px-6 py-5 space-y-4">
          {sellerName && (
            <p className="text-sm text-gray-500">
              {t('rating.rating_for')} <span className="font-medium text-primary">{sellerName}</span>
            </p>
          )}

          <div className="flex flex-col items-center gap-2">
            <StarRating value={rating} onChange={setRating} size="lg" />
            <p className="text-sm text-gray-400">
              {rating === 0 ? t('rating.select_stars') :
               rating === 1 ? t('rating.star_1') :
               rating === 2 ? t('rating.star_2') :
               rating === 3 ? t('rating.star_3') :
               rating === 4 ? t('rating.star_4') :
               t('rating.star_5')}
            </p>
          </div>

          <textarea
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder={t('rating.comment_placeholder')}
            maxLength={500}
            rows={3}
            className="w-full px-3 py-2 rounded-xl border border-lavender/40 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-primary/30 dark:bg-gray-700 dark:text-gray-100 dark:border-gray-600"
          />
        </div>

        <div className="px-6 pb-6 flex gap-3">
          <button
            onClick={onClose}
            className="flex-1 py-2.5 rounded-xl border-2 border-lavender/60 text-primary font-medium text-sm hover:bg-lavender/20 transition-colors"
          >
            {t('common.cancel')}
          </button>
          <button
            onClick={handleSubmit}
            disabled={submitting || rating === 0}
            className="flex-1 py-2.5 rounded-xl bg-primary text-white font-semibold text-sm hover:bg-primary-dark transition-colors disabled:opacity-50"
          >
            {submitting ? '…' : t('rating.submit')}
          </button>
        </div>
      </div>
    </div>
  );
}
