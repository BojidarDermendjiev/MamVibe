/**
 * Maps a category name to its default placeholder image path.
 * Used when an item has no uploaded photos.
 */
const categoryImageMap: Record<string, string> = {
  clothes: '/categories/clothes.svg',
  strollers: '/categories/strollers.svg',
  'car seats': '/categories/car-seats.svg',
  toys: '/categories/toys.svg',
  furniture: '/categories/furniture.svg',
  other: '/categories/other.svg',
};

export function getCategoryImage(categoryName?: string | null): string {
  if (!categoryName) return '/categories/other.svg';
  const key = categoryName.toLowerCase();
  return categoryImageMap[key] || '/categories/other.svg';
}
