import { useState, useEffect, useCallback } from 'react';
import { itemsApi } from '../api/itemsApi';
import { useDebounce } from './useDebounce';
import type { Item, ItemFilter } from '../types/item';

interface UseItemsOptions {
  initialFilter?: Partial<ItemFilter>;
  debounceDelay?: number;
}

interface UseItemsReturn {
  items: Item[];
  totalPages: number;
  loading: boolean;
  filter: ItemFilter;
  setFilter: (partial: Partial<ItemFilter>) => void;
  searchTerm: string;
  setSearchTerm: (term: string) => void;
  refetch: () => void;
}

const defaultFilter: ItemFilter = {
  page: 1,
  pageSize: 12,
  sortBy: 'newest',
};

export function useItems(options: UseItemsOptions = {}): UseItemsReturn {
  const { initialFilter = {}, debounceDelay = 300 } = options;

  const [items, setItems] = useState<Item[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filter, setFilterState] = useState<ItemFilter>({
    ...defaultFilter,
    ...initialFilter,
  });

  const debouncedSearch = useDebounce(searchTerm, debounceDelay);

  const setFilter = useCallback((partial: Partial<ItemFilter>) => {
    setFilterState((prev) => ({ ...prev, ...partial }));
  }, []);

  const fetchItems = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await itemsApi.getAll({
        ...filter,
        searchTerm: debouncedSearch || undefined,
      });
      setItems(data.items);
      setTotalPages(data.totalPages);
    } catch {
      /* ignore */
    } finally {
      setLoading(false);
    }
  }, [filter, debouncedSearch]);

  useEffect(() => {
    fetchItems();
  }, [fetchItems]);

  return {
    items,
    totalPages,
    loading,
    filter,
    setFilter,
    searchTerm,
    setSearchTerm,
    refetch: fetchItems,
  };
}
