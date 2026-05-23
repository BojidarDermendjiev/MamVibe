import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ShoppingBag, PackageOpen, Search } from 'lucide-react';
import { motion } from 'framer-motion';
import { usePageSEO } from '@/hooks/useSEO';
import { itemsApi } from '../api/itemsApi';
import { useCategories } from '../hooks/useCategories';
import { useItems } from '../hooks/useItems';
import { useAuthStore } from '../store/authStore';
import ItemCard from '../components/items/ItemCard';
import ItemFilters from '../components/items/ItemFilters';
import Pagination from '../components/common/Pagination';
import Modal from '../components/common/Modal';
import { AgeGroup } from '../types/item';

const AGE_QUERY_MAP: Record<string, AgeGroup> = {
  newborn: AgeGroup.Newborn,
  infant: AgeGroup.Infant,
  toddler: AgeGroup.Toddler,
  preschool: AgeGroup.Preschool,
  kids: AgeGroup.SchoolAge,
};

function SkeletonItemCard() {
  return (
    <div className="bg-[#ffffff] dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/5 shadow-sm animate-pulse overflow-hidden">
      <div className="h-48 bg-gray-200 dark:bg-white/10" />
      <div className="p-4 space-y-2">
        <div className="h-4 bg-gray-200 dark:bg-white/10 rounded w-3/4" />
        <div className="h-3 bg-gray-200 dark:bg-white/10 rounded w-1/2" />
        <div className="h-5 bg-gray-200 dark:bg-white/10 rounded w-1/3 mt-2" />
      </div>
    </div>
  );
}

export default function BrowseItemsPage() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const { categories } = useCategories();

  const currentPage = Number(searchParams.get('page') ?? 1);
  const hasFilters = Boolean(
    searchParams.get('category') ||
    searchParams.get('search') ||
    searchParams.get('age') ||
    searchParams.get('listingType'),
  );

  usePageSEO({
    title: "Browse Baby Items for Sale & Donation",
    description:
      "Browse hundreds of second-hand baby clothes, strollers, toys, car seats and more. Filter by category, age group, or price. Free to browse on MamVibe.",
    canonical: "https://mamvibe.com/browse",
    index: currentPage === 1 && !hasFilters,
    structuredData: {
      "@context": "https://schema.org",
      "@type": "CollectionPage",
      name: "Browse Baby Items",
      description: "Browse second-hand baby items for sale or donation on MamVibe.",
      url: "https://mamvibe.com/browse",
    },
  });

  const { isAuthenticated } = useAuthStore();
  const [showLoginModal, setShowLoginModal] = useState(false);

  const { items, totalPages, loading, filter, setFilter, searchTerm, setSearchTerm } = useItems({
    initialFilter: {
      categoryId: searchParams.get('category') || undefined,
      ageGroup: AGE_QUERY_MAP[searchParams.get('age') ?? ''],
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
    <div>
      {/* Hero */}
      <div className="bg-[#FAF3EE] dark:bg-[#2d2a42] py-12 px-4 mb-8">
        <div className="max-w-7xl mx-auto">
          <motion.div
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
          >
            <div className="flex items-center gap-3 mb-2">
              <div
                className="w-10 h-10 rounded-2xl flex items-center justify-center"
                style={{ backgroundColor: "rgba(148,92,103,0.12)" }}
              >
                <ShoppingBag className="w-5 h-5 text-primary" />
              </div>
              <h1 className="text-3xl font-bold text-primary-dark">
                {t('nav.browse')}
              </h1>
            </div>
            <p className="text-gray-500 dark:text-gray-400 text-sm max-w-md">
              {t('browse.subtitle') || "Second-hand baby items — browse, filter, and find exactly what you need."}
            </p>
          </motion.div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 pb-8">
        <form onSubmit={handleSearch} className="mb-6 flex gap-3">
          <div className="relative flex-1">
            <Search size={15} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
            <input
              type="text"
              placeholder={t('common.search')}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 dark:border-white/8 bg-[#ffffff] dark:bg-[#2d2a42] text-sm text-gray-800 dark:text-gray-100 placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-primary/30 shadow-sm transition"
            />
          </div>
          <button
            type="submit"
            className="px-5 py-2.5 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary/90 transition-colors shadow-sm"
          >
            {t('common.search')}
          </button>
        </form>

        <div className="flex flex-col lg:flex-row gap-6">
          <aside className="lg:w-64 flex-shrink-0">
            <ItemFilters filter={filter} categories={categories} onChange={handleFilterChange} />
          </aside>

          <div className="flex-1">
            {loading ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
                {Array.from({ length: 6 }).map((_, i) => (
                  <SkeletonItemCard key={i} />
                ))}
              </div>
            ) : items.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-24 text-center">
                <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center mb-4">
                  <PackageOpen className="w-8 h-8 text-primary/60" />
                </div>
                <p className="text-gray-500 dark:text-gray-400 font-medium">
                  {t('items.no_items')}
                </p>
                <p className="text-gray-400 dark:text-gray-500 text-sm mt-1">
                  {t('browse.noItemsHint') || "Try adjusting your filters or search term."}
                </p>
              </div>
            ) : (
              <>
                <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
                  {items.map((item, i) => (
                    <motion.div
                      key={item.id}
                      initial={{ opacity: 0, y: 16 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ duration: 0.3, delay: i * 0.04 }}
                    >
                      <ItemCard
                        item={item}
                        onLikeToggle={handleLikeToggle}
                        onRequireAuth={!isAuthenticated ? handleRequireAuth : undefined}
                      />
                    </motion.div>
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
