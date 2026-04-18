import { useState, useEffect, useCallback, useRef } from 'react';
import { itemsApi } from '@/api/itemsApi';
import { useDebounce } from './useDebounce';
import type { Item, ItemFilter } from '@mamvibe/shared';

const DEFAULT_FILTER: ItemFilter = {
  page: 1,
  pageSize: 12,
  sortBy: 'newest',
};

export function useItems(initialFilter: Partial<ItemFilter> = {}) {
  const [items, setItems] = useState<Item[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filter, setFilterState] = useState<ItemFilter>({ ...DEFAULT_FILTER, ...initialFilter });

  const debouncedSearch = useDebounce(searchTerm, 300);
  const appendNextRef = useRef(false);

  const setFilter = useCallback((partial: Partial<ItemFilter>) => {
    setFilterState((prev) => ({ ...prev, ...partial }));
  }, []);

  const fetchItems = useCallback(async (append = false) => {
    if (append) setLoadingMore(true);
    else setLoading(true);
    setError(null);
    try {
      const { data } = await itemsApi.getAll({
        ...filter,
        searchTerm: debouncedSearch || undefined,
      });
      setItems((prev) => append ? [...prev, ...data.items] : data.items);
      setTotalPages(data.totalPages);
    } catch {
      setError('Failed to load items');
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, [filter, debouncedSearch]);

  useEffect(() => {
    const append = appendNextRef.current;
    appendNextRef.current = false;
    fetchItems(append);
  }, [fetchItems]);

  const loadNextPage = () => {
    if (filter.page < totalPages && !loadingMore) {
      appendNextRef.current = true;
      setFilter({ page: filter.page + 1 });
    }
  };

  return {
    items,
    totalPages,
    loading,
    loadingMore,
    error,
    filter,
    setFilter,
    searchTerm,
    setSearchTerm,
    refetch: () => fetchItems(false),
    loadNextPage,
  };
}
