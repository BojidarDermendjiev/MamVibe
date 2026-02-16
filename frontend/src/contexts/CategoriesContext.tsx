import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { itemsApi } from '../api/itemsApi';
import type { Category } from '../types/item';

interface CategoriesContextValue {
  categories: Category[];
  loading: boolean;
}

const CategoriesContext = createContext<CategoriesContextValue>({
  categories: [],
  loading: true,
});

export function CategoriesProvider({ children }: { children: ReactNode }) {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    itemsApi
      .getCategories()
      .then((res) => setCategories(res.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <CategoriesContext.Provider value={{ categories, loading }}>
      {children}
    </CategoriesContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useCategories(): CategoriesContextValue {
  return useContext(CategoriesContext);
}
