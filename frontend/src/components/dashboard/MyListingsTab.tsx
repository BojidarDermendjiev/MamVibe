import { useTranslation } from 'react-i18next';
import type { Item } from '@/types/item';
import ItemCard from '@/components/items/ItemCard';
import TabErrorState from './TabErrorState';

interface MyListingsTabProps {
  items: Item[];
  error: string | null;
  onRetry: () => void;
  onLikeToggle: (id: string) => void;
  onBump: (id: string) => void;
}

export default function MyListingsTab({ items, error, onRetry, onLikeToggle, onBump }: MyListingsTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-listings" aria-labelledby="tab-listings">
      {items.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t('dashboard.no_listings')}</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {items.map((item) => (
            <ItemCard key={item.id} item={item} onLikeToggle={onLikeToggle} onBump={onBump} showStatus />
          ))}
        </div>
      )}
    </div>
  );
}
