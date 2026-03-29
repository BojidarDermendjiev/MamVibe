import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { itemsApi } from '../api/itemsApi';
import { useCategories } from '../hooks/useCategories';
import { useItems } from '../hooks/useItems';
import { useAuthStore } from '../store/authStore';
import ItemCard from '../components/items/ItemCard';
import ItemFilters from '../components/items/ItemFilters';
import Pagination from '../components/common/Pagination';
import LoadingSpinner from '../components/common/LoadingSpinner';
import Input from '../components/common/Input';
import Modal from '../components/common/Modal';

export default function BrowseItemsPage() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const { categories } = useCategories();
  const { isAuthenticated } = useAuthStore();
  const [showLoginModal, setShowLoginModal] = useState(false);

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

  const handleRequireAuth = () => {
    setShowLoginModal(true);
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
                  <ItemCard
                    key={item.id}
                    item={item}
                    onLikeToggle={handleLikeToggle}
                    onRequireAuth={!isAuthenticated ? handleRequireAuth : undefined}
                  />
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
      <Modal
        isOpen={showLoginModal}
        onClose={() => setShowLoginModal(false)}
        title={t('auth.login_required_title')}
      >
        <div className="text-center py-2">
          <div className="text-4xl mb-4">🔒</div>
          <p className="text-text mb-6">{t('auth.login_required_desc')}</p>
          <div className="flex flex-col sm:flex-row gap-3">
            <Link
              to="/login"
              onClick={() => setShowLoginModal(false)}
              className="flex-1 bg-mauve text-white py-2.5 px-4 rounded-lg font-medium hover:bg-mauve/90 transition-colors text-center"
            >
              {t('nav.login')}
            </Link>
            <Link
              to="/register"
              onClick={() => setShowLoginModal(false)}
              className="flex-1 border border-mauve text-mauve py-2.5 px-4 rounded-lg font-medium hover:bg-mauve/10 transition-colors text-center"
            >
              {t('nav.register')}
            </Link>
          </div>
        </div>
      </Modal>
    </div>
  );
}
