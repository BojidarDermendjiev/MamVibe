import { useState } from 'react';
import { Bookmark } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import toast from '../../utils/toast';
import { savedSearchesApi } from '../../api/savedSearchesApi';
import { useAuthStore } from '../../store/authStore';
import type { ItemFilter } from '../../types/item';
import type { CreateSavedSearchDto } from '../../types/savedSearch';
import Modal from '../common/Modal';

interface SaveSearchButtonProps {
  filter: ItemFilter;
  searchTerm: string;
  onRequireAuth?: () => void;
}

export default function SaveSearchButton({ filter, searchTerm, onRequireAuth }: SaveSearchButtonProps) {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuthStore();
  const [showModal, setShowModal] = useState(false);
  const [name, setName] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [saving, setSaving] = useState(false);

  const hasFilters = Boolean(
    filter.categoryId || searchTerm || filter.listingType != null ||
    filter.ageGroup != null || filter.shoeSize || filter.clothingSize || filter.condition != null
  );

  if (!hasFilters) return null;

  const handleOpen = () => {
    if (!isAuthenticated) { onRequireAuth?.(); return; }
    const parts: string[] = [];
    if (searchTerm) parts.push(`"${searchTerm}"`);
    if (filter.ageGroup != null) parts.push(t(`items.age_group_${filter.ageGroup}`, { defaultValue: '' }));
    if (!parts.length) parts.push(t('saved_search.default_name'));
    setName(parts.join(' · '));
    setMaxPrice('');
    setShowModal(true);
  };

  const handleSave = async () => {
    if (!name.trim()) return;
    setSaving(true);
    const dto: CreateSavedSearchDto = {
      name: name.trim(),
      categoryId: filter.categoryId ?? null,
      listingType: filter.listingType ?? null,
      searchTerm: searchTerm || null,
      ageGroup: filter.ageGroup ?? null,
      shoeSize: filter.shoeSize ?? null,
      clothingSize: filter.clothingSize ?? null,
      condition: filter.condition ?? null,
      maxPrice: maxPrice ? parseFloat(maxPrice) : null,
    };
    try {
      await savedSearchesApi.create(dto);
      toast.success(t('saved_search.saved_success'));
      setShowModal(false);
    } catch {
      toast.error(t('saved_search.save_error'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <>
      <button
        type="button"
        onClick={handleOpen}
        className="flex items-center gap-1.5 px-3 py-2.5 rounded-xl border border-gray-200 dark:border-white/8 bg-white dark:bg-[#2d2a42] text-sm text-gray-600 dark:text-gray-300 hover:border-mauve hover:text-mauve transition-colors shadow-sm"
      >
        <Bookmark size={15} />
        {t('saved_search.save_button')}
      </button>

      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title={t('saved_search.modal_title')}>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t('saved_search.name_label')}
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              maxLength={100}
              className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#1e1c2e] text-sm focus:outline-none focus:ring-2 focus:ring-mauve/30"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t('saved_search.max_price_label')}
            </label>
            <input
              type="number"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value)}
              min="0"
              step="0.01"
              placeholder={t('saved_search.max_price_placeholder')}
              className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#1e1c2e] text-sm focus:outline-none focus:ring-2 focus:ring-mauve/30"
            />
          </div>
          <div className="flex gap-3 pt-2">
            <button
              onClick={() => setShowModal(false)}
              className="flex-1 py-2 rounded-lg border border-gray-200 dark:border-white/10 text-sm text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
            >
              {t('common.cancel')}
            </button>
            <button
              onClick={handleSave}
              disabled={saving || !name.trim()}
              className="flex-1 py-2 rounded-lg bg-mauve text-white text-sm font-medium hover:bg-mauve/90 disabled:opacity-60 transition-colors"
            >
              {saving ? t('common.saving') : t('saved_search.save_button')}
            </button>
          </div>
        </div>
      </Modal>
    </>
  );
}
