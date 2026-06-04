export const mockUser = {
  id: 'user-e2e-1',
  email: 'e2e@example.com',
  displayName: 'E2E Tester',
  avatarUrl: null,
  bio: null,
  phoneNumber: null,
  languagePreference: 'en',
  roles: ['User'],
  isBlocked: false,
  iban: null,
  isOnHoliday: false,
  profileType: 1,
};

export const mockSellItem = {
  id: 'item-sell-1',
  title: 'Baby Winter Jacket',
  description: 'Warm jacket in excellent condition, barely worn.',
  categoryId: 'cat-1',
  categoryName: 'Clothing',
  listingType: 1, // Sell
  price: 45,
  userId: 'seller-1',
  userDisplayName: 'Jane Doe',
  userAvatarUrl: null,
  condition: 2, // LikeNew
  viewCount: 12,
  likeCount: 3,
  isLikedByCurrentUser: false,
  photos: [{ id: 'photo-1', url: '/placeholder.jpg', displayOrder: 0 }],
  createdAt: new Date().toISOString(),
  isActive: true,
  isSold: false,
  isReserved: false,
  bumpedAt: null,
  ageGroup: 1,
  clothingSize: 80,
  shoeSize: null,
};

export const mockDonateItem = {
  ...mockSellItem,
  id: 'item-donate-1',
  title: 'Soft Stuffed Bunny',
  listingType: 0, // Donate
  price: null,
};

export const mockItemsPage = {
  items: [mockSellItem, mockDonateItem],
  totalCount: 2,
  page: 1,
  pageSize: 12,
  totalPages: 1,
};

export const mockEmptyItemsPage = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 12,
  totalPages: 0,
};

export const mockLoginResponse = {
  accessToken: 'mock-access-token-e2e',
  user: mockUser,
};

export const mockStats = {
  activeListings: 1234,
  totalSellers: 456,
  happyFamilies: 789,
};

export const mockCategories = [
  { id: 'cat-1', name: 'Clothing', slug: 'clothing' },
  { id: 'cat-2', name: 'Toys', slug: 'toys' },
];
