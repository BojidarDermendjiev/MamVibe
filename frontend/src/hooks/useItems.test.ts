import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor, act } from '@testing-library/react'
import { useItems } from './useItems'
import { itemsApi } from '../api/itemsApi'
import type { Item, PagedResult } from '../types/item'
import { ListingType } from '../types/item'

vi.mock('../api/itemsApi', () => ({
  itemsApi: { getAll: vi.fn() },
}))

const mockGetAll = vi.mocked(itemsApi.getAll)

const mockItem: Item = {
  id: 'i1', title: 'Test', description: '', categoryId: 'c1', categoryName: 'Toys',
  listingType: ListingType.Sell, ageGroup: null, shoeSize: null, clothingSize: null,
  price: 10, userId: 'u1', userDisplayName: 'User', userAvatarUrl: null,
  isActive: true, viewCount: 0, likeCount: 0, isLikedByCurrentUser: false,
  photos: [], createdAt: '2024-01-01T00:00:00Z', aiModerationStatus: 1,
  aiModerationNotes: null, aiModerationScore: null,
}

const mockPagedResult: PagedResult<Item> = {
  items: [mockItem], totalCount: 1, page: 1, pageSize: 12, totalPages: 3,
}

beforeEach(() => {
  mockGetAll.mockClear()
})

describe('useItems', () => {
  it('starts in loading state', () => {
    mockGetAll.mockReturnValue(new Promise(() => {}) as ReturnType<typeof itemsApi.getAll>)
    const { result } = renderHook(() => useItems())
    expect(result.current.loading).toBe(true)
  })

  it('populates items after successful fetch', async () => {
    mockGetAll.mockResolvedValue({ data: mockPagedResult } as never)
    const { result } = renderHook(() => useItems())
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.items).toHaveLength(1)
    expect(result.current.items[0].title).toBe('Test')
  })

  it('sets totalPages from response', async () => {
    mockGetAll.mockResolvedValue({ data: mockPagedResult } as never)
    const { result } = renderHook(() => useItems())
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.totalPages).toBe(3)
  })

  it('sets error message on failure', async () => {
    mockGetAll.mockRejectedValue(new Error('Network error'))
    const { result } = renderHook(() => useItems())
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.error).toBe('Network error')
  })

  it('updates filter and re-fetches when setFilter called', async () => {
    mockGetAll.mockResolvedValue({ data: mockPagedResult } as never)
    const { result } = renderHook(() => useItems())
    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => result.current.setFilter({ page: 2 }))
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(mockGetAll).toHaveBeenCalledTimes(2)
  })

  it('has default filter values', async () => {
    mockGetAll.mockResolvedValue({ data: mockPagedResult } as never)
    const { result } = renderHook(() => useItems())
    expect(result.current.filter.page).toBe(1)
    expect(result.current.filter.pageSize).toBe(12)
    expect(result.current.filter.sortBy).toBe('newest')
  })
})
