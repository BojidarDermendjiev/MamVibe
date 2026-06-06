import { useTranslation } from 'react-i18next';
import type { SavedSearchDto } from '@/types/savedSearch';
import { formatPrice } from '@/utils/currency';
import TabErrorState from './TabErrorState';

interface SavedSearchesTabProps {
  savedSearches: SavedSearchDto[];
  error: string | null;
  onRetry: () => void;
  onDelete: (id: string) => void;
}

export default function SavedSearchesTab({ savedSearches, error, onRetry, onDelete }: SavedSearchesTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-saved-searches" aria-labelledby="tab-saved-searches">
      {savedSearches.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t('dashboard.saved_searches_empty')}</p>
      ) : (
        <div className="space-y-3">
          {savedSearches.map((s) => (
            <div key={s.id} className="bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 flex items-center gap-3">
              <div className="flex-1 min-w-0">
                <p className="font-medium text-primary">{s.name}</p>
                <p className="text-xs text-gray-400 mt-0.5">
                  {[s.categoryName, s.searchTerm, s.maxPrice != null ? `≤ ${formatPrice(s.maxPrice)}` : null].filter(Boolean).join(' · ')}
                </p>
              </div>
              <button
                onClick={() => onDelete(s.id)}
                className="text-xs text-red-400 hover:text-red-600 flex-shrink-0 transition-colors"
              >
                {t('common.delete')}
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
