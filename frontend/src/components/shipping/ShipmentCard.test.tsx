import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import ShipmentCard from './ShipmentCard'
import type { Shipment } from '../../types/shipping'
import { CourierProvider, DeliveryType, ShipmentStatus } from '../../types/shipping'

vi.mock('../../api/shippingApi', () => ({
  shippingApi: { getLabel: vi.fn() },
}))

const baseShipment: Shipment = {
  id: 'ship-1',
  paymentId: 'pay-1',
  itemTitle: 'Baby Jacket',
  courierProvider: CourierProvider.Econt,
  deliveryType: DeliveryType.Office,
  status: ShipmentStatus.InTransit,
  trackingNumber: 'ECT123456',
  waybillId: null,
  recipientName: 'Maria',
  recipientPhone: '0878123456',
  deliveryAddress: null,
  city: 'Sofia',
  officeId: null,
  officeName: null,
  shippingPrice: 5.9,
  isCod: false,
  codAmount: 0,
  isInsured: false,
  insuredAmount: 0,
  weight: 0.5,
  labelUrl: null,
  createdAt: '2024-01-01T00:00:00Z',
  sellerId: 'seller-1',
}

function setup(shipment = baseShipment, userId?: string) {
  return render(
    <MemoryRouter>
      <ShipmentCard shipment={shipment} currentUserId={userId} />
    </MemoryRouter>
  )
}

describe('ShipmentCard', () => {
  it('renders item title', () => {
    setup()
    expect(screen.getByText('Baby Jacket')).toBeInTheDocument()
  })

  it('renders tracking number', () => {
    setup()
    expect(screen.getByText(/ECT123456/)).toBeInTheDocument()
  })

  it('renders courier name', () => {
    setup()
    expect(screen.getByText(/Econt/)).toBeInTheDocument()
  })

  it('renders recipient name', () => {
    setup()
    expect(screen.getByText('Maria')).toBeInTheDocument()
  })

  it('shows Sender badge when current user is seller', () => {
    setup(baseShipment, 'seller-1')
    expect(screen.getByText('shipping.role_sender')).toBeInTheDocument()
  })

  it('shows Recipient badge when current user is not seller', () => {
    setup(baseShipment, 'buyer-99')
    expect(screen.getByText('shipping.role_recipient')).toBeInTheDocument()
  })

  it('does not show label download when user is not seller', () => {
    setup(baseShipment, 'buyer-99')
    expect(screen.queryByText('shipping.download_label')).toBeNull()
  })

  it('shows label download button for seller when status is Created', () => {
    const shipment = { ...baseShipment, status: ShipmentStatus.Created }
    setup(shipment, 'seller-1')
    expect(screen.getByText(/shipping\.download_label/)).toBeInTheDocument()
  })
})
