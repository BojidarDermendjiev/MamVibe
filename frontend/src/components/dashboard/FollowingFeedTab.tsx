import { useTranslation } from 'react-i18next';
import type { Item } from '@/types/item';
import ItemCard from '@/components/items/ItemCard';
import TabErrorState from './TabErrorState';

interface FollowingFeedTabProps {
  items: Item[];
  error: string | null;
  onRetry: () => void;
}

export default function FollowingFeedTab({ items, error, onRetry }: FollowingFeedTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-following-feed" aria-labelledby="tab-following-feed">
      {items.length === 0 ? (
        <div className="text-center py-20">
          <p className="text-gray-400">{t('dashboard.following_feed_empty')}</p>
          <p className="text-sm text-gray-300 mt-1">{t('dashboard.following_feed_empty_hint')}</p>
        </div>
      ) : (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {items.map((item) => (
            <ItemCard key={item.id} item={item} />
          ))}
        </div>
      )}
    </div>
  );
}
