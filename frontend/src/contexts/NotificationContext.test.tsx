import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { Shipment } from '../types/shipping'

type ShipmentPayload = Pick<Shipment, 'id' | 'courierProvider' | 'itemTitle' | 'trackingNumber'>

const captured = vi.hoisted(() => ({
  onMessage: null as ((msg: { senderId: string }) => void) | null,
  onPurchaseRequest: null as (() => void) | null,
  onSellerShipmentReady: null as ((s: ShipmentPayload) => void) | null,
  onShipmentStatusChanged: null as ((s: ShipmentPayload) => void) | null,
}))

vi.mock('./SignalRContext', () => ({
  useSignalR: () => ({
    onMessage: (cb: (msg: { senderId: string }) => void) => {
      captured.onMessage = cb
      return () => { captured.onMessage = null }
    },
    onPurchaseRequest: (cb: () => void) => {
      captured.onPurchaseRequest = cb
      return () => { captured.onPurchaseRequest = null }
    },
    onSellerShipmentReady: (cb: (s: ShipmentPayload) => void) => {
      captured.onSellerShipmentReady = cb
      return () => { captured.onSellerShipmentReady = null }
    },
    onShipmentStatusChanged: (cb: (s: ShipmentPayload) => void) => {
      captured.onShipmentStatusChanged = cb
      return () => { captured.onShipmentStatusChanged = null }
    },
  }),
}))

let mockAuthState = { isAuthenticated: true, isLoading: false, user: { id: 'u-1' } }

vi.mock('../store/authStore', () => ({
  useAuthStore: () => mockAuthState,
}))

vi.mock('../api/messagesApi', () => ({
  messagesApi: { getConversations: vi.fn(), markAsRead: vi.fn() },
}))

vi.mock('../api/purchaseRequestsApi', () => ({
  purchaseRequestsApi: { getAsSeller: vi.fn() },
}))

vi.mock('@/utils/toast', () => ({
  default: Object.assign(vi.fn(), { dismiss: vi.fn(), success: vi.fn(), error: vi.fn() }),
}))

import { NotificationProvider, useNotification } from './NotificationContext'
import { messagesApi } from '../api/messagesApi'
import { purchaseRequestsApi } from '../api/purchaseRequestsApi'
import { PurchaseRequestStatus } from '../types/purchaseRequest'
import { CourierProvider } from '../types/shipping'
import toast from '@/utils/toast'

const mockGetConversations = vi.mocked(messagesApi.getConversations)
const mockGetAsSeller = vi.mocked(purchaseRequestsApi.getAsSeller)
const mockMarkAsRead = vi.mocked(messagesApi.markAsRead)
const mockToast = vi.mocked(toast)

function TestConsumer() {
  const { unreadCount, pendingRequestCount, markConversationRead, decrementPendingRequestCount, setActiveChatUserId } = useNotification()
  return (
    <div>
      <span data-testid="unread">{unreadCount}</span>
      <span data-testid="pending">{pendingRequestCount}</span>
      <button onClick={() => markConversationRead('u-2')}>mark read</button>
      <button onClick={decrementPendingRequestCount}>decrement</button>
      <button onClick={() => setActiveChatUserId('u-2')}>set active chat</button>
    </div>
  )
}

function setup() {
  return render(
    <NotificationProvider>
      <TestConsumer />
    </NotificationProvider>
  )
}

beforeEach(() => {
  captured.onMessage = null
  captured.onPurchaseRequest = null
  captured.onSellerShipmentReady = null
  captured.onShipmentStatusChanged = null
  mockGetConversations.mockClear()
  mockGetAsSeller.mockClear()
  mockMarkAsRead.mockClear()
  mockToast.mockClear()
  mockAuthState = { isAuthenticated: true, isLoading: false, user: { id: 'u-1' } }
})

describe('NotificationContext', () => {
  it('initializes unreadCount from conversations API', async () => {
    mockGetConversations.mockResolvedValue({ data: [{ unreadCount: 3 }, { unreadCount: 2 }] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(screen.getByTestId('unread').textContent).toBe('5'))
  })

  it('initializes pendingRequestCount from purchase requests API', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({
      data: [
        { status: PurchaseRequestStatus.Pending },
        { status: PurchaseRequestStatus.Pending },
        { status: PurchaseRequestStatus.Accepted },
      ],
    } as never)
    setup()
    await waitFor(() => expect(screen.getByTestId('pending').textContent).toBe('2'))
  })

  it('increments unreadCount on message from another user', async () => {
    mockGetConversations.mockResolvedValue({ data: [{ unreadCount: 0 }] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onMessage).not.toBeNull())
    act(() => captured.onMessage!({ senderId: 'other-user' }))
    expect(screen.getByTestId('unread').textContent).toBe('1')
  })

  it('does not increment unreadCount for own messages', async () => {
    mockGetConversations.mockResolvedValue({ data: [{ unreadCount: 0 }] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onMessage).not.toBeNull())
    act(() => captured.onMessage!({ senderId: 'u-1' }))
    expect(screen.getByTestId('unread').textContent).toBe('0')
  })

  it('does not increment unreadCount for messages from the active chat user', async () => {
    mockGetConversations.mockResolvedValue({ data: [{ unreadCount: 0 }] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onMessage).not.toBeNull())
    await userEvent.click(screen.getByText('set active chat'))
    act(() => captured.onMessage!({ senderId: 'u-2' }))
    expect(screen.getByTestId('unread').textContent).toBe('0')
  })

  it('increments pendingRequestCount when a new purchase request arrives', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onPurchaseRequest).not.toBeNull())
    act(() => captured.onPurchaseRequest!())
    expect(screen.getByTestId('pending').textContent).toBe('1')
  })

  it('decrementPendingRequestCount reduces count by 1, min 0', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [{ status: PurchaseRequestStatus.Pending }] } as never)
    setup()
    await waitFor(() => expect(screen.getByTestId('pending').textContent).toBe('1'))
    await userEvent.click(screen.getByText('decrement'))
    expect(screen.getByTestId('pending').textContent).toBe('0')
  })

  it('does not decrement below 0', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(screen.getByTestId('pending').textContent).toBe('0'))
    await userEvent.click(screen.getByText('decrement'))
    expect(screen.getByTestId('pending').textContent).toBe('0')
  })

  it('markConversationRead calls markAsRead and re-fetches unread count', async () => {
    mockGetConversations.mockResolvedValue({ data: [{ unreadCount: 2 }] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    mockMarkAsRead.mockResolvedValue({} as never)
    setup()
    await waitFor(() => expect(screen.getByTestId('unread').textContent).toBe('2'))
    mockGetConversations.mockResolvedValue({ data: [{ unreadCount: 0 }] } as never)
    await userEvent.click(screen.getByText('mark read'))
    expect(mockMarkAsRead).toHaveBeenCalledWith('u-2')
    await waitFor(() => expect(screen.getByTestId('unread').textContent).toBe('0'))
  })

  it('shows 0 counts when user is not authenticated', async () => {
    mockAuthState = { isAuthenticated: false, isLoading: false, user: { id: '' } }
    mockGetConversations.mockResolvedValue({ data: [{ unreadCount: 5 }] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [{ status: PurchaseRequestStatus.Pending }] } as never)
    setup()
    // APIs should not be called when not authenticated
    await new Promise((r) => setTimeout(r, 50))
    expect(screen.getByTestId('unread').textContent).toBe('0')
    expect(screen.getByTestId('pending').textContent).toBe('0')
    expect(mockGetConversations).not.toHaveBeenCalled()
  })

  it('fires toast when seller shipment is ready (known courier)', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onSellerShipmentReady).not.toBeNull())
    act(() => captured.onSellerShipmentReady!({
      id: 'sh-1',
      courierProvider: CourierProvider.Econt,
      itemTitle: 'Baby shoes',
      trackingNumber: 'ABC123',
    }))
    expect(mockToast).toHaveBeenCalled()
  })

  it('fires toast when seller shipment is ready (unknown courier uses empty string)', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onSellerShipmentReady).not.toBeNull())
    act(() => captured.onSellerShipmentReady!({
      id: 'sh-2',
      courierProvider: 99 as CourierProvider,
      itemTitle: null,
      trackingNumber: null,
    }))
    expect(mockToast).toHaveBeenCalled()
  })

  it('fires toast when shipment status changes (Speedy)', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onShipmentStatusChanged).not.toBeNull())
    act(() => captured.onShipmentStatusChanged!({
      id: 'sh-3',
      courierProvider: CourierProvider.Speedy,
      itemTitle: 'Stroller',
      trackingNumber: 'XYZ789',
    }))
    expect(mockToast).toHaveBeenCalled()
  })

  it('fires toast when shipment status changes (unknown courier)', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onShipmentStatusChanged).not.toBeNull())
    act(() => captured.onShipmentStatusChanged!({
      id: 'sh-4',
      courierProvider: 99 as CourierProvider,
      itemTitle: null,
      trackingNumber: null,
    }))
    expect(mockToast).toHaveBeenCalled()
  })

  it('renders seller shipment ready toast content and calls dismiss on link click', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onSellerShipmentReady).not.toBeNull())
    act(() => captured.onSellerShipmentReady!({
      id: 'sh-10',
      courierProvider: CourierProvider.Econt,
      itemTitle: 'Baby shoes',
      trackingNumber: 'ABC123',
    }))
    const renderFn = mockToast.mock.calls[0][0] as (t: { id: string }) => React.ReactElement
    const { getByText } = render(renderFn({ id: 'seller-toast-id' }))
    await userEvent.click(getByText('View waybill & download label →'))
    expect(vi.mocked(toast.dismiss)).toHaveBeenCalledWith('seller-toast-id')
  })

  it('renders buyer shipment status changed toast content and calls dismiss on link click', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await waitFor(() => expect(captured.onShipmentStatusChanged).not.toBeNull())
    act(() => captured.onShipmentStatusChanged!({
      id: 'sh-11',
      courierProvider: CourierProvider.BoxNow,
      itemTitle: 'Stroller',
      trackingNumber: 'TRACK-99',
    }))
    const renderFn = mockToast.mock.calls[0][0] as (t: { id: string }) => React.ReactElement
    const { getByText } = render(renderFn({ id: 'buyer-toast-id' }))
    await userEvent.click(getByText('Track your package →'))
    expect(vi.mocked(toast.dismiss)).toHaveBeenCalledWith('buyer-toast-id')
  })

  it('handles getConversations failure on mount gracefully', async () => {
    mockGetConversations.mockRejectedValue(new Error('Network error'))
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    setup()
    await new Promise((r) => setTimeout(r, 50))
    expect(screen.getByTestId('unread').textContent).toBe('0')
  })

  it('handles getAsSeller failure on mount gracefully', async () => {
    mockGetConversations.mockResolvedValue({ data: [] } as never)
    mockGetAsSeller.mockRejectedValue(new Error('Network error'))
    setup()
    await new Promise((r) => setTimeout(r, 50))
    expect(screen.getByTestId('pending').textContent).toBe('0')
  })

  it('handles markConversationRead when getConversations re-fetch fails', async () => {
    mockGetConversations.mockResolvedValue({ data: [{ unreadCount: 1 }] } as never)
    mockGetAsSeller.mockResolvedValue({ data: [] } as never)
    mockMarkAsRead.mockResolvedValue({} as never)
    setup()
    await waitFor(() => expect(screen.getByTestId('unread').textContent).toBe('1'))
    mockGetConversations.mockRejectedValue(new Error('Network error'))
    await userEvent.click(screen.getByText('mark read'))
    expect(mockMarkAsRead).toHaveBeenCalledWith('u-2')
    // catch handler runs without throwing; count stays at 1
    await new Promise((r) => setTimeout(r, 50))
    expect(screen.getByTestId('unread').textContent).toBe('1')
  })
})
