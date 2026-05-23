import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, act } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import ChatPage from './ChatPage'
import { messagesApi } from '../api/messagesApi'
import type { Message } from '../types/message'

// ---------------------------------------------------------------------------
// Hoisted shared state — must be declared before any vi.mock calls
// ---------------------------------------------------------------------------
const captured = vi.hoisted(() => ({
  isOnline: false,
  onMessage: null as null | ((m: Message) => void),
  onTyping: null as null | ((uid: string) => void),
  onRead: null as null | ((uid: string) => void),
  markAsRead: vi.fn<[string], Promise<void>>().mockResolvedValue(undefined),
  sendMessage: vi.fn<[string, string], Promise<Message | null>>().mockResolvedValue(null),
}))

// ---------------------------------------------------------------------------
// Mocks
// ---------------------------------------------------------------------------
vi.mock('../hooks/useSignalR', () => ({
  useSignalR: () => ({
    isConnected: true,
    isUserOnline: () => captured.isOnline,
    sendMessage: captured.sendMessage,
    sendTyping: vi.fn().mockResolvedValue(undefined),
    markAsRead: captured.markAsRead,
    onMessage: (cb: (m: Message) => void) => { captured.onMessage = cb; return () => {} },
    onTyping: (cb: (uid: string) => void) => { captured.onTyping = cb; return () => {} },
    onRead: (cb: (uid: string) => void) => { captured.onRead = cb; return () => {} },
    onOnline: () => () => {},
    onOffline: () => () => {},
    onPurchaseRequest: () => () => {},
    onPurchaseRequestUpdated: () => () => {},
    onPaymentChosen: () => () => {},
    onSellerShipmentReady: () => () => {},
    onShipmentStatusChanged: () => () => {},
  }),
}))

vi.mock('../api/messagesApi', () => ({
  messagesApi: {
    getConversations: vi.fn(),
    getMessages: vi.fn(),
    markAsRead: vi.fn().mockResolvedValue({}),
  },
}))

vi.mock('../store/authStore', () => ({
  useAuthStore: () => ({
    user: { id: 'me', displayName: 'Me', avatarUrl: null, profileType: 0 },
  }),
}))

vi.mock('../contexts/NotificationContext', () => ({
  useNotification: () => ({
    markConversationRead: vi.fn(),
    setActiveChatUserId: vi.fn(),
  }),
}))

vi.mock('@/hooks/useSEO', () => ({ usePageSEO: vi.fn() }))

// ---------------------------------------------------------------------------
// Test data
// ---------------------------------------------------------------------------
const PEER_ID = 'peer-1'

const CONVERSATIONS = [
  {
    userId: PEER_ID,
    displayName: 'Alice',
    avatarUrl: null,
    lastMessage: 'Hi there',
    lastMessageTime: new Date().toISOString(),
    unreadCount: 0,
  },
]

function makeMsg(overrides: Partial<Message> = {}): Message {
  return {
    id: 'm1',
    senderId: 'me',
    senderDisplayName: 'Me',
    senderAvatarUrl: null,
    receiverId: PEER_ID,
    content: 'Hello',
    timestamp: new Date().toISOString(),
    isRead: false,
    ...overrides,
  }
}

const mockGetConversations = vi.mocked(messagesApi.getConversations)
const mockGetMessages = vi.mocked(messagesApi.getMessages)

function setup(path = '/chat') {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/chat" element={<ChatPage />} />
        <Route path="/chat/:userId" element={<ChatPage />} />
      </Routes>
    </MemoryRouter>
  )
}

beforeEach(() => {
  captured.isOnline = false
  captured.onMessage = null
  captured.onTyping = null
  captured.onRead = null
  captured.markAsRead.mockClear()
  captured.sendMessage.mockClear()
  mockGetConversations.mockResolvedValue({ data: CONVERSATIONS } as never)
  mockGetMessages.mockResolvedValue({ data: [] } as never)
})

// ---------------------------------------------------------------------------
// Online indicator in chat header
// ---------------------------------------------------------------------------
describe('ChatPage — online indicator in header', () => {
  it('shows Offline when peer is not online', async () => {
    captured.isOnline = false
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(screen.getByText('chat.offline')).toBeInTheDocument())
  })

  it('shows Online when peer is online', async () => {
    captured.isOnline = true
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(screen.getByText('chat.online')).toBeInTheDocument())
  })

  it('shows typing indicator instead of online/offline while peer is typing', async () => {
    captured.isOnline = true
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(captured.onTyping).not.toBeNull())
    act(() => captured.onTyping!(PEER_ID))
    expect(screen.getByText('chat.typing')).toBeInTheDocument()
    expect(screen.queryByText('chat.online')).not.toBeInTheDocument()
  })
})

// ---------------------------------------------------------------------------
// Green dot in conversation sidebar
// ---------------------------------------------------------------------------
describe('ChatPage — green dot in sidebar', () => {
  it('renders a green dot badge on the avatar when peer is online', async () => {
    captured.isOnline = true
    setup()
    await waitFor(() => expect(screen.getByText('Alice')).toBeInTheDocument())
    expect(document.querySelector('.bg-green-500.rounded-full')).toBeInTheDocument()
  })

  it('does not render a green dot when peer is offline', async () => {
    captured.isOnline = false
    setup()
    await waitFor(() => expect(screen.getByText('Alice')).toBeInTheDocument())
    expect(document.querySelector('.bg-green-500.rounded-full')).not.toBeInTheDocument()
  })
})

// ---------------------------------------------------------------------------
// Read receipt checkmarks
// ---------------------------------------------------------------------------
describe('ChatPage — read receipts', () => {
  it('renders ✓ for an outgoing unread message', async () => {
    mockGetMessages.mockResolvedValue({ data: [makeMsg({ isRead: false })] } as never)
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(screen.getByText('✓')).toBeInTheDocument())
  })

  it('renders ✓✓ for an outgoing read message', async () => {
    mockGetMessages.mockResolvedValue({ data: [makeMsg({ isRead: true })] } as never)
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(screen.getByText('✓✓')).toBeInTheDocument())
  })

  it('does not render checkmarks for received messages', async () => {
    const received = makeMsg({
      id: 'm2',
      senderId: PEER_ID,
      senderDisplayName: 'Alice',
      receiverId: 'me',
      content: 'Hey from peer',
      isRead: true,
    })
    mockGetMessages.mockResolvedValue({ data: [received] } as never)
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(screen.getByText('Hey from peer')).toBeInTheDocument())
    expect(screen.queryByText('✓')).not.toBeInTheDocument()
    expect(screen.queryByText('✓✓')).not.toBeInTheDocument()
  })
})

// ---------------------------------------------------------------------------
// onRead subscription
// ---------------------------------------------------------------------------
describe('ChatPage — onRead subscription', () => {
  it('flips unread sent messages to ✓✓ when the peer reads them', async () => {
    mockGetMessages.mockResolvedValue({ data: [makeMsg({ isRead: false })] } as never)
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(screen.getByText('✓')).toBeInTheDocument())

    act(() => captured.onRead!(PEER_ID))

    expect(screen.getByText('✓✓')).toBeInTheDocument()
    expect(screen.queryByText('✓')).not.toBeInTheDocument()
  })

  it('does not flip messages when a different user triggers onRead', async () => {
    mockGetMessages.mockResolvedValue({ data: [makeMsg({ isRead: false })] } as never)
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(screen.getByText('✓')).toBeInTheDocument())

    act(() => captured.onRead!('unrelated-user'))

    expect(screen.getByText('✓')).toBeInTheDocument()
    expect(screen.queryByText('✓✓')).not.toBeInTheDocument()
  })

  it('leaves received messages unchanged when onRead fires', async () => {
    const received = makeMsg({
      id: 'm2',
      senderId: PEER_ID,
      senderDisplayName: 'Alice',
      receiverId: 'me',
      content: 'Incoming',
      isRead: false,
    })
    const sent = makeMsg({ id: 'm1', isRead: false })
    mockGetMessages.mockResolvedValue({ data: [received, sent] } as never)
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(screen.getByText('✓')).toBeInTheDocument())

    act(() => captured.onRead!(PEER_ID))

    // Sent message becomes read; received message had no checkmark and still has none
    expect(screen.getByText('✓✓')).toBeInTheDocument()
    expect(screen.queryByText('✓')).not.toBeInTheDocument()
  })
})

// ---------------------------------------------------------------------------
// Hub-based markAsRead
// ---------------------------------------------------------------------------
describe('ChatPage — hub markAsRead', () => {
  it('calls hub markAsRead when opening a conversation', async () => {
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(captured.markAsRead).toHaveBeenCalledWith(PEER_ID))
  })

  it('calls hub markAsRead when an incoming message arrives from the active peer', async () => {
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(captured.onMessage).not.toBeNull())
    captured.markAsRead.mockClear()

    act(() => captured.onMessage!(makeMsg({
      id: 'm-new',
      senderId: PEER_ID,
      senderDisplayName: 'Alice',
      receiverId: 'me',
      content: 'Hey!',
      isRead: false,
    })))

    expect(captured.markAsRead).toHaveBeenCalledWith(PEER_ID)
  })

  it('does not call hub markAsRead when a message arrives from a different user', async () => {
    setup(`/chat/${PEER_ID}`)
    await waitFor(() => expect(captured.onMessage).not.toBeNull())
    captured.markAsRead.mockClear()

    act(() => captured.onMessage!(makeMsg({
      id: 'm-other',
      senderId: 'other-user',
      senderDisplayName: 'Bob',
      receiverId: 'me',
      content: 'From Bob',
      isRead: false,
    })))

    expect(captured.markAsRead).not.toHaveBeenCalled()
  })
})
