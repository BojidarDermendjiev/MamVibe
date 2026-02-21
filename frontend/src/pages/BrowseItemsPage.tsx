import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { itemsApi } from '../api/itemsApi';
import { useCategories } from '../hooks/useCategories';
import { useItems } from '../hooks/useItems';
import ItemCard from '../components/items/ItemCard';
import ItemFilters from '../components/items/ItemFilters';
import Pagination from '../components/common/Pagination';
import LoadingSpinner from '../components/common/LoadingSpinner';
import Input from '../components/common/Input';

export default function BrowseItemsPage() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const { categories } = useCategories();

  const { items, totalPages, loading, filter, setFilter, searchTerm, setSearchTerm } = useItems({
    initialFilter: {
      categoryId: searchParams.get('category') || undefined,
      page: 1,
      pageSize: 12,
      sortBy: 'newest',
    },
  });

  const handleFilterChange = (partial: Partial<typeof filter>) => {
    setFilter(partial);
  };

  const handleLikeToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
    } catch { /* ignore */ }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setFilter({ page: 1 });
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 animate-fade-in">
      <h1 className="text-3xl font-bold mb-6 text-[#364153] dark:text-[#bdb9bc]">{t('nav.browse')}</h1>

      <form onSubmit={handleSearch} className="mb-6">
        <Input
          placeholder={t('common.search')}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </form>

      <div className="flex flex-col lg:flex-row gap-6">
        <aside className="lg:w-64 flex-shrink-0">
          <ItemFilters filter={filter} categories={categories} onChange={handleFilterChange} />
        </aside>

        <div className="flex-1">
          {loading ? (
            <LoadingSpinner size="lg" className="py-20" />
          ) : items.length === 0 ? (
            <div className="text-center py-20 text-gray-400">
              <span className="text-4xl block mb-2">📦</span>
              {t('items.no_items')}
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
                {items.map((item) => (
                  <ItemCard key={item.id} item={item} onLikeToggle={handleLikeToggle} />
                ))}
              </div>
              <Pagination
                currentPage={filter.page}
                totalPages={totalPages}
                onPageChange={(page) => handleFilterChange({ page })}
              />
            </>
          )}
        </div>
      </div>
    </div>
  );
}
