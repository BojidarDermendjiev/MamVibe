import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import PaymentPage from './PaymentPage'
import { itemsApi } from '../api/itemsApi'
import { paymentsApi } from '../api/paymentsApi'
import { ListingType } from '../types/item'

vi.mock('../api/itemsApi', () => ({ itemsApi: { getById: vi.fn() } }))
vi.mock('../api/paymentsApi', () => ({ paymentsApi: { createBooking: vi.fn(), createCheckout: vi.fn(), createCod: vi.fn() } }))
vi.mock('../utils/toast', () => ({ default: { error: vi.fn(), success: vi.fn() } }))

// Avoid real HTTP calls from sub-components
vi.mock('../components/shipping/OfficePicker', () => ({
  default: ({ onChange }: { onChange: (id: string, name: string) => void }) => (
    <button onClick={() => onChange('OFF-1', 'Sofia Office')}>Select Office</button>
  ),
}))
vi.mock('../components/shipping/ShippingPricePreview', () => ({
  default: () => <div data-testid="price-preview" />,
}))

const sellItem = {
  id: 'item-sell',
  title: 'Baby Stroller',
  description: 'Good',
  price: 80,
  listingType: ListingType.Sell,
  isActive: true,
  isReserved: false,
  isSold: false,
  photos: [],
  userId: 'seller-1',
  userDisplayName: 'Seller',
  userAvatarUrl: null,
  userIsOnHoliday: false,
  condition: 0,
  categoryId: 'c1',
  categoryName: 'strollers',
  ageGroup: null,
  shoeSize: null,
  clothingSize: null,
  viewCount: 0,
  likeCount: 0,
  isLikedByCurrentUser: false,
  bumpedAt: null,
  createdAt: '2024-01-01',
  aiModerationStatus: 1,
  aiModerationNotes: null,
  aiModerationScore: null,
}

const donateItem = { ...sellItem, id: 'item-donate', listingType: ListingType.Donate, price: null }

function setup(itemId: string) {
  return render(
    <MemoryRouter initialEntries={[`/payment/${itemId}`]}>
      <Routes>
        <Route path="/payment/:itemId" element={<PaymentPage />} />
        <Route path="/payment/:itemId/card" element={<div>CardPage</div>} />
        <Route path="/payment/success" element={<div>SuccessPage</div>} />
      </Routes>
    </MemoryRouter>
  )
}

beforeEach(() => {
  vi.mocked(itemsApi.getById).mockResolvedValue({ data: sellItem } as never)
})

describe('PaymentPage', () => {
  it('renders page title', async () => {
    setup('item-sell')
    await waitFor(() => expect(screen.getByText('payment.title')).toBeInTheDocument())
  })

  it('shows item title and price for sell item', async () => {
    setup('item-sell')
    await waitFor(() => {
      expect(screen.getByText('Baby Stroller')).toBeInTheDocument()
      // Price appears in both the item row and the total row — use getAllByText
      expect(screen.getAllByText(/80/).length).toBeGreaterThan(0)
    })
  })

  it('shows "Free" label for donate item', async () => {
    vi.mocked(itemsApi.getById).mockResolvedValue({ data: donateItem } as never)
    setup('item-donate')
    await waitFor(() => expect(screen.getByText('items.free')).toBeInTheDocument())
  })

  it('shows "Confirm Booking" button for donate item', async () => {
    vi.mocked(itemsApi.getById).mockResolvedValue({ data: donateItem } as never)
    setup('item-donate')
    await waitFor(() => expect(screen.getByText('payment.confirm_booking')).toBeInTheDocument())
  })

  it('shows "Continue to Card" button for sell item', async () => {
    setup('item-sell')
    await waitFor(() => expect(screen.getByText('payment.continue_to_card')).toBeInTheDocument())
  })

  it('excludes Address delivery type for donate items', async () => {
    vi.mocked(itemsApi.getById).mockResolvedValue({ data: donateItem } as never)
    setup('item-donate')
    await waitFor(() => expect(screen.getByText('payment.confirm_booking')).toBeInTheDocument())
    expect(screen.queryByText('shipping.to_address')).toBeNull()
  })

  it('calls createBooking when donate item submitted with valid details', async () => {
    vi.mocked(itemsApi.getById).mockResolvedValue({ data: donateItem } as never)
    vi.mocked(paymentsApi.createBooking).mockResolvedValue({ data: {} } as never)
    setup('item-donate')

    await waitFor(() => screen.getByText('payment.confirm_booking'))

    // Select an office
    await userEvent.click(screen.getByText('Select Office'))

    // Recipient inputs have no htmlFor — find by role index (name first, phone second)
    const inputs = screen.getAllByRole('textbox')
    const nameInput = inputs[inputs.length - 2]
    const phoneInput = inputs[inputs.length - 1]
    await userEvent.type(nameInput, 'John Doe')
    await userEvent.type(phoneInput, '+359888000000')

    await userEvent.click(screen.getByText('payment.confirm_booking'))

    await waitFor(() => expect(paymentsApi.createBooking).toHaveBeenCalledWith(
      'item-donate',
      expect.objectContaining({ recipientName: 'John Doe', recipientPhone: '+359888000000' })
    ))
  })

  it('navigates to card page when sell item submitted with valid details', async () => {
    setup('item-sell')

    await waitFor(() => screen.getByText('payment.continue_to_card'))

    await userEvent.click(screen.getByText('Select Office'))
    const inputs = screen.getAllByRole('textbox')
    await userEvent.type(inputs[inputs.length - 2], 'Jane Doe')
    await userEvent.type(inputs[inputs.length - 1], '+359888111111')

    await userEvent.click(screen.getByText('payment.continue_to_card'))

    await waitFor(() => expect(screen.getByText('CardPage')).toBeInTheDocument())
  })

  it('shows payment method selector for sell items', async () => {
    setup('item-sell')
    await waitFor(() => {
      expect(screen.getByText('payment.method')).toBeInTheDocument()
      expect(screen.getByText('payment.cod')).toBeInTheDocument()
    })
  })

  it('does not show payment method selector for donate items', async () => {
    vi.mocked(itemsApi.getById).mockResolvedValue({ data: donateItem } as never)
    setup('item-donate')
    await waitFor(() => screen.getByText('payment.confirm_booking'))
    expect(screen.queryByText('payment.cod')).toBeNull()
  })

  it('shows cod_confirm button when COD method selected', async () => {
    setup('item-sell')
    await waitFor(() => screen.getByText('payment.cod'))
    await userEvent.click(screen.getByText('payment.cod'))
    expect(screen.getByText('payment.cod_confirm')).toBeInTheDocument()
  })

  it('calls createCod when COD submitted with valid details', async () => {
    vi.mocked(paymentsApi.createCod).mockResolvedValue({ data: {} } as never)
    setup('item-sell')

    await waitFor(() => screen.getByText('payment.cod'))
    await userEvent.click(screen.getByText('payment.cod'))

    await userEvent.click(screen.getByText('Select Office'))
    const inputs = screen.getAllByRole('textbox')
    await userEvent.type(inputs[inputs.length - 2], 'John Doe')
    await userEvent.type(inputs[inputs.length - 1], '+359888999000')

    await userEvent.click(screen.getByText('payment.cod_confirm'))

    await waitFor(() => expect(paymentsApi.createCod).toHaveBeenCalledWith(
      'item-sell',
      expect.objectContaining({ recipientName: 'John Doe', recipientPhone: '+359888999000' })
    ))
  })
})
