import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { BundleDto } from '@/types/bundle';
import { formatPrice } from '@/utils/currency';
import TabErrorState from './TabErrorState';

interface BundlesTabProps {
  bundles: BundleDto[];
  error: string | null;
  onRetry: () => void;
  onDelete: (id: string) => void;
  onCreateBundle: () => void;
}

export default function BundlesTab({ bundles, error, onRetry, onDelete, onCreateBundle }: BundlesTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-bundles" aria-labelledby="tab-bundles">
      <div className="flex justify-end mb-4">
        <button
          onClick={onCreateBundle}
          className="px-4 py-2 rounded-lg bg-mauve text-white text-sm font-medium hover:bg-mauve/90 transition-colors"
        >
          + {t('bundle.create_btn')}
        </button>
      </div>
      {bundles.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t('dashboard.bundles_empty')}</p>
      ) : (
        <div className="space-y-3">
          {bundles.map((b) => (
            <div key={b.id} className="bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 flex items-center gap-3 hover:shadow-md transition-shadow">
              <div className="flex-shrink-0 flex -space-x-2">
                {b.items.slice(0, 3).map((item) =>
                  item.photoUrl ? (
                    <img key={item.itemId} src={item.photoUrl} alt={item.title} className="w-12 h-12 rounded-lg object-cover border-2 border-white dark:border-[#2d2a42]" />
                  ) : (
                    <div key={item.itemId} className="w-12 h-12 rounded-lg bg-lavender/20 border-2 border-white dark:border-[#2d2a42]" />
                  )
                )}
              </div>
              <div className="flex-1 min-w-0">
                <Link to={`/bundles/${b.id}`} className="font-medium text-primary hover:underline truncate block">{b.title}</Link>
                <p className="text-xs text-gray-400 mt-0.5">
                  {t('bundle.items_count', { count: b.items.length })} · {formatPrice(b.price)}
                  {b.isSold && <span className="ml-2 text-red-500">{t('bundle.sold_badge')}</span>}
                </p>
              </div>
              {!b.isSold && (
                <button
                  onClick={() => onDelete(b.id)}
                  className="text-xs text-red-400 hover:text-red-600 flex-shrink-0 transition-colors"
                >
                  {t('common.delete')}
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
