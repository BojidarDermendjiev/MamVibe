import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import toast from '../../utils/toast';
import { formatPrice } from '../../utils/currency';
import { bundlesApi } from '../../api/bundlesApi';
import type { Item } from '../../types/item';
import Modal from '../common/Modal';

interface CreateBundleModalProps {
  isOpen: boolean;
  onClose: () => void;
  myItems: Item[];
  onCreated: () => void;
}

export default function CreateBundleModal({ isOpen, onClose, myItems, onCreated }: CreateBundleModalProps) {
  const { t } = useTranslation();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState('');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      setTitle('');
      setDescription('');
      setPrice('');
      setSelectedIds(new Set());
    }
  }, [isOpen]);

  const toggleItem = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleSave = async () => {
    if (!title.trim()) { toast.error(t('bundle.title_required')); return; }
    const parsedPrice = parseFloat(price);
    if (isNaN(parsedPrice) || parsedPrice <= 0) { toast.error(t('bundle.price_required')); return; }
    if (selectedIds.size < 2) { toast.error(t('bundle.min_items')); return; }
    if (selectedIds.size > 10) { toast.error(t('bundle.max_items')); return; }

    setSaving(true);
    try {
      await bundlesApi.create({
        title: title.trim(),
        description: description.trim() || null,
        price: parsedPrice,
        itemIds: Array.from(selectedIds),
      });
      toast.success(t('bundle.created'));
      onCreated();
      onClose();
    } catch {
      toast.error(t('bundle.create_error'));
    } finally {
      setSaving(false);
    }
  };

  const availableItems = myItems.filter((i) => i.isActive && !i.isReserved && !i.isSold);

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('bundle.create_title')}>
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            {t('bundle.title_label')}
          </label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            maxLength={150}
            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#1e1c2e] text-sm focus:outline-none focus:ring-2 focus:ring-mauve/30"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            {t('bundle.description_label')}
          </label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            maxLength={1000}
            rows={2}
            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#1e1c2e] text-sm focus:outline-none focus:ring-2 focus:ring-mauve/30 resize-none"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            {t('bundle.price_label')}
          </label>
          <div className="relative">
            <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 text-sm select-none">€</span>
            <input
              type="number"
              value={price}
              onChange={(e) => setPrice(e.target.value)}
              min="0.01"
              step="0.01"
              className="w-full pl-9 pr-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#1e1c2e] text-sm focus:outline-none focus:ring-2 focus:ring-mauve/30"
            />
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('bundle.select_items')} ({selectedIds.size}/10, {t('bundle.min_2')})
          </label>
          {availableItems.length === 0 ? (
            <p className="text-sm text-gray-400">{t('bundle.no_items_available')}</p>
          ) : (
            <div className="space-y-2 max-h-48 overflow-y-auto pr-1">
              {availableItems.map((item) => {
                const checked = selectedIds.has(item.id);
                return (
                  <label
                    key={item.id}
                    className={`flex items-center gap-3 p-2 rounded-lg border cursor-pointer transition-colors ${
                      checked
                        ? 'border-mauve bg-mauve/5'
                        : 'border-gray-200 dark:border-white/10 hover:border-mauve/40'
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() => toggleItem(item.id)}
                      className="accent-mauve"
                    />
                    {item.photos?.[0]?.url && (
                      <img src={item.photos[0].url} alt={item.title} className="w-10 h-10 rounded-md object-cover flex-shrink-0" />
                    )}
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-primary truncate">{item.title}</p>
                      <p className="text-xs text-gray-400">{item.price != null ? formatPrice(item.price) : 'Free'}</p>
                    </div>
                  </label>
                );
              })}
            </div>
          )}
        </div>
        <div className="flex gap-3 pt-2">
          <button
            onClick={onClose}
            className="flex-1 py-2 rounded-lg border border-gray-200 dark:border-white/10 text-sm text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
          >
            {t('common.cancel')}
          </button>
          <button
            onClick={handleSave}
            disabled={saving}
            className="flex-1 py-2 rounded-lg bg-mauve text-white text-sm font-medium hover:bg-mauve/90 disabled:opacity-60 transition-colors"
          >
            {saving ? t('common.saving') : t('bundle.create_btn')}
          </button>
        </div>
      </div>
    </Modal>
  );
}
