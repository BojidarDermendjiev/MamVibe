import { useEffect, useState } from 'react';
import { itemsApi } from '@/api/itemsApi';
import type { Category } from '@mamvibe/shared';

export function useCategories() {
  const [categories, setCategories] = useState<Category[]>([]);

  useEffect(() => {
    itemsApi.getCategories().then(({ data }) => setCategories(data)).catch(() => {});
  }, []);

  return { categories };
}
