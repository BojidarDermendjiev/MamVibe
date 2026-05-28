import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Modal from '../common/Modal';
import Button from '../common/Button';
import { offersApi } from '../../api/offersApi';
import toast from '../../utils/toast';
import { formatPrice } from '../../utils/currency';

interface MakeOfferModalProps {
  itemId: string;
  itemTitle: string;
  listingPrice: number;
  onClose: () => void;
  onSuccess: () => void;
}

export default function MakeOfferModal({ itemId, itemTitle, listingPrice, onClose, onSuccess }: MakeOfferModalProps) {
  const { t } = useTranslation();
  const [price, setPrice] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const parsed = parseFloat(price);
    if (isNaN(parsed) || parsed <= 0) {
      toast.error(t('offer.invalid_price'));
      return;
    }
    setSubmitting(true);
    try {
      await offersApi.create(itemId, parsed);
      toast.success(t('offer.sent'));
      onSuccess();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        t('offer.send_error');
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen onClose={onClose} title={t('offer.title')}>
      <div className="py-2">
        <p className="text-sm text-gray-500 mb-1">{itemTitle}</p>
        <p className="text-sm text-gray-500 mb-5">
          {t('offer.listing_price')}: <span className="font-semibold text-mauve">{formatPrice(listingPrice)}</span>
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t('offer.your_price')}
            </label>
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">лв.</span>
              <input
                type="number"
                step="0.01"
                value={price}
                onChange={e => setPrice(e.target.value)}
                placeholder="0.00"
                className="w-full pl-10 pr-4 py-2.5 border border-gray-200 dark:border-white/10 rounded-lg bg-white dark:bg-white/5 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary/40"
                required
              />
            </div>
          </div>

          <div className="flex gap-3 pt-1">
            <Button type="button" variant="ghost" fullWidth onClick={onClose} disabled={submitting}>
              {t('common.cancel')}
            </Button>
            <Button type="submit" fullWidth disabled={submitting}>
              {submitting ? t('common.sending') : t('offer.send')}
            </Button>
          </div>
        </form>
      </div>
    </Modal>
  );
}
