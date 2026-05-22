import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import ShipmentCard from './ShipmentCard'
import type { Shipment } from '../../types/shipping'
import { CourierProvider, DeliveryType, ShipmentStatus } from '../../types/shipping'
import { shippingApi } from '../../api/shippingApi'

vi.mock('../../api/shippingApi', () => ({
  shippingApi: { getLabel: vi.fn() },
}))

vi.mock('../../utils/toast', () => ({
  default: Object.assign(vi.fn(), { error: vi.fn(), success: vi.fn(), dismiss: vi.fn() }),
}))

const mockGetLabel = vi.mocked(shippingApi.getLabel)

beforeEach(() => {
  mockGetLabel.mockClear()
  window.URL.createObjectURL = vi.fn(() => 'blob:mock')
  window.URL.revokeObjectURL = vi.fn()
})

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

  it('shows label download button for seller when status is PickedUp', () => {
    const shipment = { ...baseShipment, status: ShipmentStatus.PickedUp }
    setup(shipment, 'seller-1')
    expect(screen.getByText(/shipping\.download_label/)).toBeInTheDocument()
  })

  it('does not show label button for seller when status is InTransit', () => {
    setup(baseShipment, 'seller-1') // baseShipment is InTransit
    expect(screen.queryByText(/shipping\.download_label/)).toBeNull()
  })

  it('shows fallback title when itemTitle is null', () => {
    setup({ ...baseShipment, itemTitle: null })
    expect(screen.getByText('shipping.shipment')).toBeInTheDocument()
  })

  it('shows dash when trackingNumber is null', () => {
    setup({ ...baseShipment, trackingNumber: null })
    expect(screen.getByText(/Econt — -/)).toBeInTheDocument()
  })

  it('shows Unknown for unrecognised courier provider', () => {
    setup({ ...baseShipment, courierProvider: 99 as CourierProvider })
    expect(screen.getByText(/Unknown/)).toBeInTheDocument()
  })

  it('shows Speedy courier name', () => {
    setup({ ...baseShipment, courierProvider: CourierProvider.Speedy })
    expect(screen.getByText(/Speedy/)).toBeInTheDocument()
  })

  it('shows Box Now courier name', () => {
    setup({ ...baseShipment, courierProvider: CourierProvider.BoxNow })
    expect(screen.getByText(/Box Now/)).toBeInTheDocument()
  })

  it('shows Pigeon Express courier name', () => {
    setup({ ...baseShipment, courierProvider: CourierProvider.PigeonExpress })
    expect(screen.getByText(/Pigeon Express/)).toBeInTheDocument()
  })

  it('shows Recipient badge when sellerId does not match', () => {
    setup({ ...baseShipment, sellerId: null })
    expect(screen.getByText('shipping.role_recipient')).toBeInTheDocument()
  })

  it('downloads label on button click (success path)', async () => {
    const shipment = { ...baseShipment, status: ShipmentStatus.Created }
    mockGetLabel.mockResolvedValue({ data: new Blob(['pdf']) } as never)
    setup(shipment, 'seller-1')
    await userEvent.click(screen.getByText(/shipping\.download_label/))
    await waitFor(() => expect(mockGetLabel).toHaveBeenCalledWith('ship-1'))
    expect(window.URL.createObjectURL).toHaveBeenCalled()
    expect(window.URL.revokeObjectURL).toHaveBeenCalled()
  })

  it('shows error toast when label download fails', async () => {
    const toast = await import('../../utils/toast')
    const shipment = { ...baseShipment, status: ShipmentStatus.Created }
    mockGetLabel.mockRejectedValue(new Error('Network error'))
    setup(shipment, 'seller-1')
    await userEvent.click(screen.getByText(/shipping\.download_label/))
    await waitFor(() => expect(vi.mocked(toast.default.error)).toHaveBeenCalledWith('shipping.label_error'))
  })

  it('disables label button while downloading', async () => {
    const shipment = { ...baseShipment, status: ShipmentStatus.Created }
    let resolveDownload!: () => void
    mockGetLabel.mockReturnValue(new Promise<never>((res) => { resolveDownload = res as never }))
    setup(shipment, 'seller-1')
    const btn = screen.getByText(/shipping\.download_label/).closest('button')!
    await userEvent.click(btn)
    expect(btn).toBeDisabled()
    resolveDownload()
  })
})
