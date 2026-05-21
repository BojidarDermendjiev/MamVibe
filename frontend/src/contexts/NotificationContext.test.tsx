import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'

// Capture SignalR handler callbacks so tests can trigger them
const captured = vi.hoisted(() => ({
  onMessage: null as ((msg: { senderId: string }) => void) | null,
  onPurchaseRequest: null as (() => void) | null,
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
    onSellerShipmentReady: () => () => {},
    onShipmentStatusChanged: () => () => {},
  }),
}))

vi.mock('../store/authStore', () => ({
  useAuthStore: () => ({ isAuthenticated: true, isLoading: false, user: { id: 'u-1' } }),
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

const mockGetConversations = vi.mocked(messagesApi.getConversations)
const mockGetAsSeller = vi.mocked(purchaseRequestsApi.getAsSeller)
const mockMarkAsRead = vi.mocked(messagesApi.markAsRead)

function TestConsumer({ onDecrement: _onDecrement }: { onDecrement?: () => void }) {
  const { unreadCount, pendingRequestCount, markConversationRead, decrementPendingRequestCount } = useNotification()
  return (
    <div>
      <span data-testid="unread">{unreadCount}</span>
      <span data-testid="pending">{pendingRequestCount}</span>
      <button onClick={() => markConversationRead('u-2')}>mark read</button>
      <button onClick={decrementPendingRequestCount}>decrement</button>
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
  mockGetConversations.mockClear()
  mockGetAsSeller.mockClear()
  mockMarkAsRead.mockClear()
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
})
