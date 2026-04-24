import { useTranslation } from 'react-i18next';
import { Check, X } from 'lucide-react';
import { ListingType, AgeGroup, type Category, type ItemFilter } from '../../types/item';

const BRANDS = [
  'Cybex', 'Little Dutch', 'Nuna', 'Bugaboo', 'Ergobaby',
  'UPPAbaby', 'Joie', 'Chicco', 'Maxi-Cosi', 'BabyBjörn',
  'Stokke', 'Graco', 'Britax', 'Mamas & Papas', 'Skip Hop',
  'Hauck', 'Peg Perego', 'BABYZEN',
];

const AGE_GROUPS: { label: string; value: AgeGroup }[] = [
  { label: 'Newborn',    value: AgeGroup.Newborn },
  { label: 'Infant',     value: AgeGroup.Infant },
  { label: 'Toddler',    value: AgeGroup.Toddler },
  { label: 'Preschool',  value: AgeGroup.Preschool },
  { label: 'School Age', value: AgeGroup.SchoolAge },
  { label: 'Teen',       value: AgeGroup.Teen },
];

const SORT_OPTIONS: { label: string; value: string }[] = [
  { label: 'Newest first',       value: 'newest' },
  { label: 'Oldest first',       value: 'oldest' },
  { label: 'Price: low to high', value: 'price_asc' },
  { label: 'Price: high to low', value: 'price_desc' },
  { label: 'Most popular',       value: 'most_liked' },
];

interface CheckRowProps {
  label: string;
  selected: boolean;
  onToggle: () => void;
}

function CheckRow({ label, selected, onToggle }: CheckRowProps) {
  return (
    <button
      onClick={onToggle}
      className="flex items-center gap-3 w-full py-2 text-left hover:opacity-75 transition-opacity"
    >
      <span
        className={`w-[18px] h-[18px] rounded-[4px] border flex-shrink-0 flex items-center justify-center transition-colors ${
          selected
            ? 'bg-primary border-primary'
            : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-transparent'
        }`}
      >
        {selected && <Check className="w-3 h-3 text-white stroke-[3]" />}
      </span>
      <span className="text-sm text-gray-700 dark:text-gray-300">{label}</span>
    </button>
  );
}

function Divider() {
  return <hr className="border-gray-200 dark:border-gray-700 my-4" />;
}

interface ItemFiltersProps {
  filter: ItemFilter;
  categories: Category[];
  onChange: (filter: Partial<ItemFilter>) => void;
}

export default function ItemFilters({ filter, categories, onChange }: ItemFiltersProps) {
  const { t } = useTranslation();

  const hasActiveFilters =
    !!filter.categoryId ||
    filter.listingType !== undefined ||
    !!filter.brand ||
    filter.ageGroup !== undefined ||
    filter.sortBy !== 'newest';

  const clearAll = () =>
    onChange({ categoryId: undefined, listingType: undefined, brand: undefined, ageGroup: undefined, sortBy: 'newest', page: 1 });

  return (
    <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden">

      {/* Header */}
      <div className="flex items-center justify-between px-5 py-4 border-b border-gray-200 dark:border-gray-700">
        <span className="font-bold text-[15px] text-gray-800 dark:text-gray-100">Filters</span>
        {hasActiveFilters && (
          <button
            onClick={clearAll}
            className="flex items-center gap-1 text-xs text-gray-400 hover:text-primary transition-colors"
          >
            <X className="w-3 h-3" />
            Clear all
          </button>
        )}
      </div>

      <div className="px-5 py-4">

        {/* ── Category ── */}
        <p className="font-bold text-[15px] text-gray-800 dark:text-gray-100 mb-1">{t('items.category')}</p>
        <CheckRow
          label={t('items.all_categories')}
          selected={!filter.categoryId}
          onToggle={() => onChange({ categoryId: undefined, page: 1 })}
        />
        {categories.map((cat) => (
          <CheckRow
            key={cat.id}
            label={cat.name}
            selected={filter.categoryId === cat.id}
            onToggle={() => onChange({ categoryId: filter.categoryId === cat.id ? undefined : cat.id, page: 1 })}
          />
        ))}

        <Divider />

        {/* ── Listing Type ── */}
        <p className="font-bold text-[15px] text-gray-800 dark:text-gray-100 mb-1">{t('items.listing_type')}</p>
        <CheckRow
          label="All"
          selected={filter.listingType === undefined}
          onToggle={() => onChange({ listingType: undefined, page: 1 })}
        />
        <CheckRow
          label={t('items.sell')}
          selected={filter.listingType === ListingType.Sell}
          onToggle={() => onChange({ listingType: filter.listingType === ListingType.Sell ? undefined : ListingType.Sell, page: 1 })}
        />
        <CheckRow
          label={t('items.donate')}
          selected={filter.listingType === ListingType.Donate}
          onToggle={() => onChange({ listingType: filter.listingType === ListingType.Donate ? undefined : ListingType.Donate, page: 1 })}
        />

        <Divider />

        {/* ── Age Group ── */}
        <p className="font-bold text-[15px] text-gray-800 dark:text-gray-100 mb-1">Age Group</p>
        <CheckRow
          label="All ages"
          selected={filter.ageGroup === undefined}
          onToggle={() => onChange({ ageGroup: undefined, page: 1 })}
        />
        {AGE_GROUPS.map((ag) => (
          <CheckRow
            key={ag.value}
            label={ag.label}
            selected={filter.ageGroup === ag.value}
            onToggle={() => onChange({ ageGroup: filter.ageGroup === ag.value ? undefined : ag.value, page: 1 })}
          />
        ))}

        <Divider />

        {/* ── Brand ── */}
        <p className="font-bold text-[15px] text-gray-800 dark:text-gray-100 mb-1">Brand</p>
        <div className="max-h-48 overflow-y-auto">
          <CheckRow
            label="All brands"
            selected={!filter.brand}
            onToggle={() => onChange({ brand: undefined, page: 1 })}
          />
          {BRANDS.map((b) => (
            <CheckRow
              key={b}
              label={b}
              selected={filter.brand === b}
              onToggle={() => onChange({ brand: filter.brand === b ? undefined : b, page: 1 })}
            />
          ))}
        </div>

        <Divider />

        {/* ── Sort ── */}
        <p className="font-bold text-[15px] text-gray-800 dark:text-gray-100 mb-1">Sort By</p>
        {SORT_OPTIONS.map((opt) => (
          <CheckRow
            key={opt.value}
            label={opt.label}
            selected={filter.sortBy === opt.value}
            onToggle={() => onChange({ sortBy: opt.value, page: 1 })}
          />
        ))}

      </div>
    </div>
  );
}
