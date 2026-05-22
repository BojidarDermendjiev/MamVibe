import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SignalRProvider, useSignalR } from './SignalRContext'
import { signalRService } from '../services/signalRService'
import { useAuthStore } from '../store/authStore'

vi.mock('../services/signalRService', () => ({
  signalRService: {
    connect: vi.fn(),
    disconnect: vi.fn(),
    sendMessage: vi.fn(),
    sendTyping: vi.fn(),
    markAsRead: vi.fn(),
    onMessage: vi.fn(),
    onTyping: vi.fn(),
    onRead: vi.fn(),
    onOnline: vi.fn(),
    onOffline: vi.fn(),
    onPurchaseRequest: vi.fn(),
    onPurchaseRequestUpdated: vi.fn(),
    onPaymentChosen: vi.fn(),
    onSellerShipmentReady: vi.fn(),
    onShipmentStatusChanged: vi.fn(),
  },
}))

vi.mock('../store/authStore', () => ({
  useAuthStore: vi.fn(),
}))

const mockConnect = vi.mocked(signalRService.connect)
const mockDisconnect = vi.mocked(signalRService.disconnect)
const mockUseAuthStore = vi.mocked(useAuthStore)

function ConnectedConsumer() {
  const { isConnected } = useSignalR()
  return <span data-testid="connected">{String(isConnected)}</span>
}

function ActionConsumer() {
  const { sendMessage, sendTyping, markAsRead, onMessage } = useSignalR()
  return (
    <>
      <button aria-label="send" onClick={() => sendMessage('r1', 'hello')} />
      <button aria-label="typing" onClick={() => sendTyping('r1')} />
      <button aria-label="read" onClick={() => markAsRead('s1')} />
      <button aria-label="subscribe" onClick={() => onMessage(() => {})} />
    </>
  )
}

function authed(token = 'tok') {
  mockUseAuthStore.mockReturnValue({ isAuthenticated: true, accessToken: token } as never)
}

function unauthed() {
  mockUseAuthStore.mockReturnValue({ isAuthenticated: false, accessToken: null } as never)
}

beforeEach(() => {
  mockConnect.mockClear()
  mockDisconnect.mockClear()
  mockConnect.mockResolvedValue(undefined as never)
  mockDisconnect.mockResolvedValue(undefined as never)
})

describe('SignalRProvider', () => {
  it('calls signalRService.connect with the access token when authenticated', async () => {
    authed('my-token')
    render(<SignalRProvider><ConnectedConsumer /></SignalRProvider>)
    await waitFor(() => expect(mockConnect).toHaveBeenCalledWith('my-token'))
  })

  it('sets isConnected to true after successful connection', async () => {
    authed()
    render(<SignalRProvider><ConnectedConsumer /></SignalRProvider>)
    await waitFor(() =>
      expect(screen.getByTestId('connected')).toHaveTextContent('true')
    )
  })

  it('does not call connect when not authenticated', () => {
    unauthed()
    render(<SignalRProvider><ConnectedConsumer /></SignalRProvider>)
    expect(mockConnect).not.toHaveBeenCalled()
  })

  it('calls signalRService.disconnect when not authenticated', () => {
    unauthed()
    render(<SignalRProvider><ConnectedConsumer /></SignalRProvider>)
    expect(mockDisconnect).toHaveBeenCalled()
  })

  it('keeps isConnected false when connection fails', async () => {
    authed()
    mockConnect.mockRejectedValue(new Error('refused'))
    render(<SignalRProvider><ConnectedConsumer /></SignalRProvider>)
    await waitFor(() => expect(mockConnect).toHaveBeenCalled())
    expect(screen.getByTestId('connected')).toHaveTextContent('false')
  })

  it('does not double-connect on re-render while already connecting', async () => {
    authed()
    let resolve!: () => void
    mockConnect.mockReturnValue(new Promise<never>((r) => { resolve = r as () => void }))
    const { rerender } = render(<SignalRProvider><ConnectedConsumer /></SignalRProvider>)
    rerender(<SignalRProvider><ConnectedConsumer /></SignalRProvider>)
    expect(mockConnect).toHaveBeenCalledOnce()
    resolve()
  })

  it('sendMessage delegates to signalRService.sendMessage', async () => {
    vi.mocked(signalRService.sendMessage).mockResolvedValue(null)
    unauthed()
    render(<SignalRProvider><ActionConsumer /></SignalRProvider>)
    await userEvent.click(screen.getByLabelText('send'))
    expect(signalRService.sendMessage).toHaveBeenCalledWith('r1', 'hello')
  })

  it('sendTyping delegates to signalRService.sendTyping', async () => {
    vi.mocked(signalRService.sendTyping).mockResolvedValue()
    unauthed()
    render(<SignalRProvider><ActionConsumer /></SignalRProvider>)
    await userEvent.click(screen.getByLabelText('typing'))
    expect(signalRService.sendTyping).toHaveBeenCalledWith('r1')
  })

  it('markAsRead delegates to signalRService.markAsRead', async () => {
    vi.mocked(signalRService.markAsRead).mockResolvedValue()
    unauthed()
    render(<SignalRProvider><ActionConsumer /></SignalRProvider>)
    await userEvent.click(screen.getByLabelText('read'))
    expect(signalRService.markAsRead).toHaveBeenCalledWith('s1')
  })

  it('onMessage delegates to signalRService.onMessage and returns its result', async () => {
    const cleanup = vi.fn()
    vi.mocked(signalRService.onMessage).mockReturnValue(cleanup)
    unauthed()
    render(<SignalRProvider><ActionConsumer /></SignalRProvider>)
    await userEvent.click(screen.getByLabelText('subscribe'))
    expect(signalRService.onMessage).toHaveBeenCalled()
  })
})

describe('useSignalR default context', () => {
  it('returns isConnected=false from default context when used outside provider', () => {
    function Bare() {
      const { isConnected } = useSignalR()
      return <span data-testid="bare">{String(isConnected)}</span>
    }
    render(<Bare />)
    expect(screen.getByTestId('bare')).toHaveTextContent('false')
  })
})
