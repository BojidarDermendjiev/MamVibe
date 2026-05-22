import { describe, it, expect, vi, beforeEach } from 'vitest'
import { signalRService } from './signalRService'

// Only the leaf methods need to be vi.fn — the builder chain is plain functions
const mocks = vi.hoisted(() => {
  const mockInvoke = vi.fn().mockResolvedValue(undefined)
  const mockOn = vi.fn()
  const mockStart = vi.fn().mockResolvedValue(undefined)
  const mockStop = vi.fn().mockResolvedValue(undefined)

  const mockConn = {
    state: 'Disconnected' as string,
    on: mockOn,
    start: mockStart,
    stop: mockStop,
    invoke: mockInvoke,
  }

  return { mockConn, mockOn, mockStart, mockStop, mockInvoke }
})

vi.mock('@microsoft/signalr', () => ({
  // Must be a regular function (not arrow) to support `new HubConnectionBuilder()`
  HubConnectionBuilder: function HubConnectionBuilder() {
    return {
      withUrl: () => ({
        withAutomaticReconnect: () => ({
          configureLogging: () => ({
            build: () => mocks.mockConn,
          }),
        }),
      }),
    }
  },
  HubConnectionState: { Connected: 'Connected', Disconnected: 'Disconnected' },
  LogLevel: { Warning: 3 },
}))

function getHubCallback(event: string) {
  const args = mocks.mockOn.mock.calls.find(([e]) => e === event)
  if (!args) throw new Error(`No handler registered for event: ${event}`)
  return args[1] as (...args: unknown[]) => void
}

beforeEach(async () => {
  // Disconnect resets this.connection = null inside the service
  await signalRService.disconnect()
  vi.clearAllMocks()
  mocks.mockConn.state = 'Disconnected'
  mocks.mockStart.mockResolvedValue(undefined)
  mocks.mockStop.mockResolvedValue(undefined)
  mocks.mockInvoke.mockResolvedValue(undefined)
})

describe('SignalRService', () => {
  describe('connect', () => {
    it('builds a connection and calls start', async () => {
      await signalRService.connect('my-token')
      expect(mocks.mockStart).toHaveBeenCalledOnce()
    })

    it('registers all hub event handlers on connect', async () => {
      await signalRService.connect('tok')
      const registeredEvents = mocks.mockOn.mock.calls.map(([event]) => event)
      expect(registeredEvents).toContain('ReceiveMessage')
      expect(registeredEvents).toContain('MessageRead')
      expect(registeredEvents).toContain('UserTyping')
      expect(registeredEvents).toContain('UserOnline')
      expect(registeredEvents).toContain('UserOffline')
      expect(registeredEvents).toContain('ReceivePurchaseRequest')
      expect(registeredEvents).toContain('PurchaseRequestUpdated')
      expect(registeredEvents).toContain('PaymentMethodChosen')
      expect(registeredEvents).toContain('ShipmentCreated')
      expect(registeredEvents).toContain('ShipmentStatusChanged')
    })

    it('skips start when already connected', async () => {
      await signalRService.connect('tok')
      mocks.mockConn.state = 'Connected'
      mocks.mockStart.mockClear()
      await signalRService.connect('tok')
      expect(mocks.mockStart).not.toHaveBeenCalled()
    })
  })

  describe('disconnect', () => {
    it('calls stop when connected', async () => {
      await signalRService.connect('tok')
      mocks.mockStop.mockClear()
      await signalRService.disconnect()
      expect(mocks.mockStop).toHaveBeenCalledOnce()
    })

    it('is a no-op when not connected', async () => {
      await signalRService.disconnect()
      expect(mocks.mockStop).not.toHaveBeenCalled()
    })

    it('allows reconnect after disconnect', async () => {
      await signalRService.connect('tok')
      await signalRService.disconnect()
      mocks.mockStart.mockClear()
      mocks.mockConn.state = 'Disconnected'
      await signalRService.connect('tok2')
      expect(mocks.mockStart).toHaveBeenCalledOnce()
    })
  })

  describe('sendMessage', () => {
    it('invokes SendMessage with receiverId and content', async () => {
      await signalRService.connect('tok')
      mocks.mockInvoke.mockResolvedValue({ id: 'm1', content: 'hello' })
      const result = await signalRService.sendMessage('rx-id', 'hello')
      expect(mocks.mockInvoke).toHaveBeenCalledWith('SendMessage', 'rx-id', 'hello')
      expect(result).toEqual({ id: 'm1', content: 'hello' })
    })

    it('returns null when not connected', async () => {
      const result = await signalRService.sendMessage('rx-id', 'hello')
      expect(result).toBeNull()
    })
  })

  describe('markAsRead', () => {
    it('invokes MarkAsRead with senderId', async () => {
      await signalRService.connect('tok')
      await signalRService.markAsRead('sender-123')
      expect(mocks.mockInvoke).toHaveBeenCalledWith('MarkAsRead', 'sender-123')
    })
  })

  describe('sendTyping', () => {
    it('invokes SendTyping with receiverId', async () => {
      await signalRService.connect('tok')
      await signalRService.sendTyping('rx-id')
      expect(mocks.mockInvoke).toHaveBeenCalledWith('SendTyping', 'rx-id')
    })
  })

  describe('onMessage', () => {
    it('dispatches ReceiveMessage events to registered handler', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onMessage(handler)
      getHubCallback('ReceiveMessage')({ id: 'm1', content: 'hi' })
      expect(handler).toHaveBeenCalledWith({ id: 'm1', content: 'hi' })
    })

    it('unsubscribing stops further dispatch', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      const unsub = signalRService.onMessage(handler)
      unsub()
      getHubCallback('ReceiveMessage')({ id: 'm2', content: 'world' })
      expect(handler).not.toHaveBeenCalled()
    })

    it('supports multiple handlers simultaneously', async () => {
      await signalRService.connect('tok')
      const h1 = vi.fn()
      const h2 = vi.fn()
      signalRService.onMessage(h1)
      signalRService.onMessage(h2)
      getHubCallback('ReceiveMessage')({ id: 'm3' })
      expect(h1).toHaveBeenCalled()
      expect(h2).toHaveBeenCalled()
    })
  })

  describe('onRead', () => {
    it('dispatches MessageRead events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onRead(handler)
      getHubCallback('MessageRead')('user-123')
      expect(handler).toHaveBeenCalledWith('user-123')
    })

    it('unsubscribing stops dispatch', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      const unsub = signalRService.onRead(handler)
      unsub()
      getHubCallback('MessageRead')('user-123')
      expect(handler).not.toHaveBeenCalled()
    })
  })

  describe('onTyping', () => {
    it('dispatches UserTyping events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onTyping(handler)
      getHubCallback('UserTyping')('user-456')
      expect(handler).toHaveBeenCalledWith('user-456')
    })
  })

  describe('onOnline / onOffline', () => {
    it('dispatches UserOnline events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onOnline(handler)
      getHubCallback('UserOnline')('user-1')
      expect(handler).toHaveBeenCalledWith('user-1')
    })

    it('dispatches UserOffline events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onOffline(handler)
      getHubCallback('UserOffline')('user-2')
      expect(handler).toHaveBeenCalledWith('user-2')
    })

    it('unsubscribing onOnline stops dispatch', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      const unsub = signalRService.onOnline(handler)
      unsub()
      getHubCallback('UserOnline')('user-1')
      expect(handler).not.toHaveBeenCalled()
    })
  })

  describe('onPurchaseRequest', () => {
    it('dispatches ReceivePurchaseRequest events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onPurchaseRequest(handler)
      getHubCallback('ReceivePurchaseRequest')({ id: 'pr-1' })
      expect(handler).toHaveBeenCalledWith({ id: 'pr-1' })
    })

    it('unsubscribing stops dispatch', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      const unsub = signalRService.onPurchaseRequest(handler)
      unsub()
      getHubCallback('ReceivePurchaseRequest')({ id: 'pr-1' })
      expect(handler).not.toHaveBeenCalled()
    })
  })

  describe('onPurchaseRequestUpdated', () => {
    it('dispatches PurchaseRequestUpdated events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onPurchaseRequestUpdated(handler)
      getHubCallback('PurchaseRequestUpdated')({ id: 'pr-2' })
      expect(handler).toHaveBeenCalledWith({ id: 'pr-2' })
    })
  })

  describe('onPaymentChosen', () => {
    it('dispatches PaymentMethodChosen events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onPaymentChosen(handler)
      getHubCallback('PaymentMethodChosen')({ itemId: 'i-1' })
      expect(handler).toHaveBeenCalledWith({ itemId: 'i-1' })
    })

    it('unsubscribing stops dispatch', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      const unsub = signalRService.onPaymentChosen(handler)
      unsub()
      getHubCallback('PaymentMethodChosen')({ itemId: 'i-1' })
      expect(handler).not.toHaveBeenCalled()
    })
  })

  describe('onSellerShipmentReady', () => {
    it('dispatches ShipmentCreated events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onSellerShipmentReady(handler)
      getHubCallback('ShipmentCreated')({ id: 'sh-1', courierProvider: 0 })
      expect(handler).toHaveBeenCalledWith({ id: 'sh-1', courierProvider: 0 })
    })

    it('unsubscribing stops dispatch', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      const unsub = signalRService.onSellerShipmentReady(handler)
      unsub()
      getHubCallback('ShipmentCreated')({ id: 'sh-1' })
      expect(handler).not.toHaveBeenCalled()
    })
  })

  describe('onShipmentStatusChanged', () => {
    it('dispatches ShipmentStatusChanged events', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      signalRService.onShipmentStatusChanged(handler)
      getHubCallback('ShipmentStatusChanged')({ id: 'sh-2', status: 2 })
      expect(handler).toHaveBeenCalledWith({ id: 'sh-2', status: 2 })
    })

    it('unsubscribing stops dispatch', async () => {
      await signalRService.connect('tok')
      const handler = vi.fn()
      const unsub = signalRService.onShipmentStatusChanged(handler)
      unsub()
      getHubCallback('ShipmentStatusChanged')({ id: 'sh-2' })
      expect(handler).not.toHaveBeenCalled()
    })
  })
})
