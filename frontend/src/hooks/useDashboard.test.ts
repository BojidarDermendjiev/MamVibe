import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor, act } from '@testing-library/react'
import { useDashboard } from './useDashboard'
import { itemsApi } from '../api/itemsApi'
import { paymentsApi } from '../api/paymentsApi'
import { shippingApi } from '../api/shippingApi'
import { ebillsApi } from '../api/ebillsApi'
import { purchaseRequestsApi } from '../api/purchaseRequestsApi'

vi.mock('../api/itemsApi', () => ({
  itemsApi: { getMyItems: vi.fn(), getLikedItems: vi.fn() },
}))
vi.mock('../api/paymentsApi', () => ({
  paymentsApi: { getMyPayments: vi.fn() },
}))
vi.mock('../api/shippingApi', () => ({
  shippingApi: { getMyShipments: vi.fn() },
}))
vi.mock('../api/ebillsApi', () => ({
  ebillsApi: { getMyEBills: vi.fn() },
}))
vi.mock('../api/purchaseRequestsApi', () => ({
  purchaseRequestsApi: { getAsSeller: vi.fn(), getAsBuyer: vi.fn() },
}))

beforeEach(() => {
  vi.mocked(itemsApi.getMyItems).mockResolvedValue({ data: [] } as never)
  vi.mocked(itemsApi.getLikedItems).mockResolvedValue({ data: [] } as never)
  vi.mocked(paymentsApi.getMyPayments).mockResolvedValue({ data: [] } as never)
  vi.mocked(shippingApi.getMyShipments).mockResolvedValue({ data: [] } as never)
  vi.mocked(ebillsApi.getMyEBills).mockResolvedValue({ data: [] } as never)
  vi.mocked(purchaseRequestsApi.getAsSeller).mockResolvedValue({ data: [] } as never)
  vi.mocked(purchaseRequestsApi.getAsBuyer).mockResolvedValue({ data: [] } as never)
})

describe('useDashboard', () => {
  it('starts on listings tab', async () => {
    const { result } = renderHook(() => useDashboard())
    expect(result.current.tab).toBe('listings')
  })

  it('calls getMyItems for listings tab', async () => {
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(itemsApi.getMyItems).toHaveBeenCalled()
  })

  it('calls getMyPayments when tab set to purchases', async () => {
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => result.current.setTab('purchases'))
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(paymentsApi.getMyPayments).toHaveBeenCalled()
  })

  it('calls getMyShipments when tab set to shipments', async () => {
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => result.current.setTab('shipments'))
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(shippingApi.getMyShipments).toHaveBeenCalled()
  })

  it('removeLikedItem removes item from likedItems', async () => {
    vi.mocked(itemsApi.getLikedItems).mockResolvedValue({ data: [{ id: 'i1' }, { id: 'i2' }] } as never)
    const { result } = renderHook(() => useDashboard())
    act(() => result.current.setTab('liked'))
    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => result.current.removeLikedItem('i1'))
    expect(result.current.likedItems).toHaveLength(1)
    expect(result.current.likedItems[0].id).toBe('i2')
  })

  it('calls getAsSeller when tab set to incoming-requests', async () => {
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => result.current.setTab('incoming-requests'))
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(purchaseRequestsApi.getAsSeller).toHaveBeenCalled()
  })

  it('calls getAsBuyer when tab set to my-requests', async () => {
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => result.current.setTab('my-requests'))
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(purchaseRequestsApi.getAsBuyer).toHaveBeenCalled()
  })

  it('calls getMyEBills when tab set to ebills', async () => {
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => result.current.setTab('ebills'))
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(ebillsApi.getMyEBills).toHaveBeenCalled()
  })

  it('calls getLikedItems when tab set to liked', async () => {
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => result.current.setTab('liked'))
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(itemsApi.getLikedItems).toHaveBeenCalled()
  })

  it('refreshTab re-runs the current tab fetch', async () => {
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    const callsBefore = vi.mocked(itemsApi.getMyItems).mock.calls.length
    act(() => result.current.refreshTab())
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(vi.mocked(itemsApi.getMyItems).mock.calls.length).toBeGreaterThan(callsBefore)
  })

  it('sets loading false even when API throws', async () => {
    vi.mocked(itemsApi.getMyItems).mockRejectedValue(new Error('Network error'))
    const { result } = renderHook(() => useDashboard())
    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.myItems).toEqual([])
  })
})
