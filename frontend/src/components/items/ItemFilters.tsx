import { useTranslation } from 'react-i18next';
import { ListingType, type Category, type ItemFilter } from '../../types/item';

interface ItemFiltersProps {
  filter: ItemFilter;
  categories: Category[];
  onChange: (filter: Partial<ItemFilter>) => void;
}

export default function ItemFilters({ filter, categories, onChange }: ItemFiltersProps) {
  const { t } = useTranslation();

  return (
    <div className="bg-white rounded-xl p-4 border border-lavender/30 space-y-4">
      <h3 className="font-semibold text-primary">{t('items.filters')}</h3>

      <div>
        <label className="block text-sm font-medium text-gray-600 mb-1">{t('items.category')}</label>
        <select
          value={filter.categoryId || ''}
          onChange={(e) => onChange({ categoryId: e.target.value || undefined, page: 1 })}
          className="w-full px-3 py-2 rounded-lg border border-lavender bg-white text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        >
          <option value="">{t('items.all_categories')}</option>
          {categories.map((cat) => (
            <option key={cat.id} value={cat.id}>{cat.name}</option>
          ))}
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-600 mb-1">{t('items.listing_type')}</label>
        <select
          value={filter.listingType ?? ''}
          onChange={(e) => onChange({ listingType: e.target.value ? Number(e.target.value) as ListingType : undefined, page: 1 })}
          className="w-full px-3 py-2 rounded-lg border border-lavender bg-white text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        >
          <option value="">{t('items.all_categories')}</option>
          <option value={ListingType.Donate}>{t('items.donate')}</option>
          <option value={ListingType.Sell}>{t('items.sell')}</option>
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-600 mb-1">Sort</label>
        <select
          value={filter.sortBy}
          onChange={(e) => onChange({ sortBy: e.target.value, page: 1 })}
          className="w-full px-3 py-2 rounded-lg border border-lavender bg-white text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        >
          <option value="newest">{t('items.sort_newest')}</option>
          <option value="oldest">{t('items.sort_oldest')}</option>
          <option value="price_asc">{t('items.sort_price_asc')}</option>
          <option value="price_desc">{t('items.sort_price_desc')}</option>
          <option value="popular">{t('items.sort_popular')}</option>
        </select>
      </div>
    </div>
  );
}
