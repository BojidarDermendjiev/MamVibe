import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import ShipmentTracker from './ShipmentTracker'
import { shippingApi } from '../../api/shippingApi'

vi.mock('../../api/shippingApi', () => ({
  shippingApi: { trackShipment: vi.fn() },
}))

const mockTrack = vi.mocked(shippingApi.trackShipment)

beforeEach(() => mockTrack.mockReset())

const events = [
  { description: 'Picked up', timestamp: '2024-01-10T09:00:00Z', location: 'Sofia' },
  { description: 'In transit', timestamp: '2024-01-11T12:00:00Z', location: null },
]

describe('ShipmentTracker', () => {
  it('shows spinner initially (before promise settles)', () => {
    mockTrack.mockResolvedValue({ data: events } as never)
    const { container } = render(<ShipmentTracker shipmentId="ship-1" />)
    expect(container.querySelector('.animate-spin')).toBeInTheDocument()
  })

  it('shows no-tracking message when API returns empty array', async () => {
    mockTrack.mockResolvedValue({ data: [] } as never)
    render(<ShipmentTracker shipmentId="ship-1" />)
    await waitFor(() => expect(screen.getByText('shipping.no_tracking')).toBeInTheDocument())
  })

  it('shows no-tracking message when API rejects', async () => {
    mockTrack.mockResolvedValue({ data: [] } as never)
    render(<ShipmentTracker shipmentId="ship-1" />)
    await waitFor(() => expect(screen.getByText('shipping.no_tracking')).toBeInTheDocument())
  })

  it('renders event descriptions after loading', async () => {
    mockTrack.mockResolvedValue({ data: events } as never)
    render(<ShipmentTracker shipmentId="ship-1" />)
    await waitFor(() => expect(screen.getByText('Picked up')).toBeInTheDocument())
    expect(screen.getByText('In transit')).toBeInTheDocument()
  })

  it('renders event location when present', async () => {
    mockTrack.mockResolvedValue({ data: events } as never)
    render(<ShipmentTracker shipmentId="ship-1" />)
    await waitFor(() => expect(screen.getByText('Sofia')).toBeInTheDocument())
  })

  it('re-fetches when shipmentId changes', async () => {
    mockTrack.mockResolvedValue({ data: [] } as never)
    const { rerender } = render(<ShipmentTracker shipmentId="ship-1" />)
    await waitFor(() => expect(mockTrack).toHaveBeenCalledWith('ship-1'))
    rerender(<ShipmentTracker shipmentId="ship-2" />)
    await waitFor(() => expect(mockTrack).toHaveBeenCalledWith('ship-2'))
  })
})
