import { useTranslation } from 'react-i18next';
import { SlidersHorizontal, Tag, ShoppingBag, ArrowUpDown, Sparkles, Baby, X } from 'lucide-react';
import { ListingType, AgeGroup, type Category, type ItemFilter } from '../../types/item';

const BRANDS = [
  { name: 'Cybex',         domain: 'cybex-online.com' },
  { name: 'Little Dutch',  domain: 'little-dutch.com' },
  { name: 'Nuna',          domain: 'nunababy.com' },
  { name: 'Bugaboo',       domain: 'bugaboo.com' },
  { name: 'Ergobaby',      domain: 'ergobaby.com' },
  { name: 'UPPAbaby',      domain: 'uppababy.com' },
  { name: 'Joie',          domain: 'joiebaby.com' },
  { name: 'Chicco',        domain: 'chicco.com' },
  { name: 'Maxi-Cosi',     domain: 'maxi-cosi.com' },
  { name: 'BabyBjörn',     domain: 'babybjorn.com' },
  { name: 'Stokke',        domain: 'stokke.com' },
  { name: 'Graco',         domain: 'graco.com' },
  { name: 'Britax',        domain: 'britax.co.uk' },
  { name: 'Mamas & Papas', domain: 'mamasandpapas.com' },
  { name: 'Skip Hop',      domain: 'skiphop.com' },
  { name: 'Hauck',         domain: 'hauck.de' },
  { name: 'Peg Perego',    domain: 'pegperego.com' },
  { name: 'BABYZEN',       domain: 'babyzen.com' },
];

function faviconUrl(domain: string) {
  return `https://www.google.com/s2/favicons?domain=${domain}&sz=32`;
}

const AGE_GROUPS: { label: string; emoji: string; value: AgeGroup }[] = [
  { label: 'Newborn',    emoji: '👶', value: AgeGroup.Newborn },
  { label: 'Infant',     emoji: '🍼', value: AgeGroup.Infant },
  { label: 'Toddler',    emoji: '🧸', value: AgeGroup.Toddler },
  { label: 'Preschool',  emoji: '🎨', value: AgeGroup.Preschool },
  { label: 'School Age', emoji: '📚', value: AgeGroup.SchoolAge },
  { label: 'Teen',       emoji: '🎒', value: AgeGroup.Teen },
];

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
    <div className="bg-white rounded-2xl border border-lavender/30 shadow-sm overflow-hidden">

      {/* ── Header ── */}
      <div className="px-4 py-3 flex items-center justify-between bg-gradient-to-r from-peach-light/60 to-lavender-light/40 border-b border-lavender/20">
        <div className="flex items-center gap-2">
          <SlidersHorizontal className="w-4 h-4 text-primary" />
          <h3 className="font-semibold text-sm text-primary">{t('items.filters')}</h3>
        </div>
        {hasActiveFilters && (
          <button
            onClick={clearAll}
            className="flex items-center gap-1 text-xs text-gray-400 hover:text-primary transition-colors px-2 py-0.5 rounded-full hover:bg-peach-light/60"
          >
            <X className="w-3 h-3" />
            Clear
          </button>
        )}
      </div>

      <div className="p-4 space-y-5">

        {/* ── Category ── */}
        <div>
          <label className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-gray-400 mb-2">
            <Tag className="w-3.5 h-3.5" />
            {t('items.category')}
          </label>
          <select
            value={filter.categoryId || ''}
            onChange={(e) => onChange({ categoryId: e.target.value || undefined, page: 1 })}
            className="w-full px-3 py-2 rounded-xl border border-lavender/50 bg-peach-light/20 text-sm text-primary focus:outline-none focus:ring-2 focus:ring-lavender focus:border-transparent transition-all"
          >
            <option value="">{t('items.all_categories')}</option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>{cat.name}</option>
            ))}
          </select>
        </div>

        {/* ── Listing Type ── */}
        <div>
          <label className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-gray-400 mb-2">
            <ShoppingBag className="w-3.5 h-3.5" />
            {t('items.listing_type')}
          </label>
          <div className="flex gap-1.5">
            {([
              { label: 'All',              value: undefined },
              { label: t('items.donate'),  value: ListingType.Donate },
              { label: t('items.sell'),    value: ListingType.Sell },
            ] as { label: string; value: ListingType | undefined }[]).map(({ label, value }) => (
              <button
                key={String(value)}
                onClick={() => onChange({ listingType: value, page: 1 })}
                className={`flex-1 py-1.5 rounded-xl text-xs font-medium transition-all ${
                  filter.listingType === value
                    ? 'bg-primary text-white shadow-sm'
                    : 'bg-peach-light/50 text-primary hover:bg-lavender/40'
                }`}
              >
                {label}
              </button>
            ))}
          </div>
        </div>

        {/* ── Brands ── */}
        <div>
          <label className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-gray-400 mb-2">
            <Sparkles className="w-3.5 h-3.5" />
            Brand
          </label>
          <div className="flex flex-wrap gap-1.5 max-h-52 overflow-y-auto">

            {/* All pill */}
            <button
              onClick={() => onChange({ brand: undefined, page: 1 })}
              className={`px-2.5 py-1 rounded-full text-xs font-medium transition-all ${
                !filter.brand
                  ? 'bg-primary text-white shadow-sm'
                  : 'bg-peach-light/60 text-primary hover:bg-lavender/40'
              }`}
            >
              All
            </button>

            {BRANDS.map((b) => (
              <button
                key={b.name}
                onClick={() => onChange({ brand: filter.brand === b.name ? undefined : b.name, page: 1 })}
                className={`flex items-center gap-1 pl-1 pr-2.5 py-1 rounded-full text-xs font-medium transition-all ${
                  filter.brand === b.name
                    ? 'bg-primary text-white shadow-sm'
                    : 'bg-peach-light/60 text-primary hover:bg-lavender/40'
                }`}
              >
                <img
                  src={faviconUrl(b.domain)}
                  alt=""
                  className="w-3.5 h-3.5 rounded-sm flex-shrink-0"
                  onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
                />
                <span>{b.name}</span>
              </button>
            ))}

          </div>
        </div>

        {/* ── Age Group ── */}
        <div>
          <label className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-gray-400 mb-2">
            <Baby className="w-3.5 h-3.5" />
            Age
          </label>
          <div className="flex flex-wrap gap-1.5">

            {/* All pill */}
            <button
              onClick={() => onChange({ ageGroup: undefined, page: 1 })}
              className={`px-2.5 py-1 rounded-full text-xs font-medium transition-all ${
                filter.ageGroup === undefined
                  ? 'bg-primary text-white shadow-sm'
                  : 'bg-peach-light/60 text-primary hover:bg-lavender/40'
              }`}
            >
              All
            </button>

            {AGE_GROUPS.map((ag) => (
              <button
                key={ag.value}
                onClick={() => onChange({ ageGroup: filter.ageGroup === ag.value ? undefined : ag.value, page: 1 })}
                className={`flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium transition-all ${
                  filter.ageGroup === ag.value
                    ? 'bg-primary text-white shadow-sm'
                    : 'bg-peach-light/60 text-primary hover:bg-lavender/40'
                }`}
              >
                <span>{ag.emoji}</span>
                <span>{ag.label}</span>
              </button>
            ))}

          </div>
        </div>

        {/* ── Sort ── */}
        <div>
          <label className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-gray-400 mb-2">
            <ArrowUpDown className="w-3.5 h-3.5" />
            Sort
          </label>
          <select
            value={filter.sortBy}
            onChange={(e) => onChange({ sortBy: e.target.value, page: 1 })}
            className="w-full px-3 py-2 rounded-xl border border-lavender/50 bg-peach-light/20 text-sm text-primary focus:outline-none focus:ring-2 focus:ring-lavender focus:border-transparent transition-all"
          >
            <option value="newest">{t('items.sort_newest')}</option>
            <option value="oldest">{t('items.sort_oldest')}</option>
            <option value="price_asc">{t('items.sort_price_asc')}</option>
            <option value="price_desc">{t('items.sort_price_desc')}</option>
            <option value="most_liked">{t('items.sort_popular')}</option>
          </select>
        </div>

      </div>
    </div>
  );
}
