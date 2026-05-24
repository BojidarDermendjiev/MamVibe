import { describe, it, expect, vi, beforeEach } from 'vitest'
import userEvent from '@testing-library/user-event'
import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import PaymentSuccessPage from './PaymentSuccessPage'
import { purchaseRequestsApi } from '../api/purchaseRequestsApi'
import { PurchaseRequestStatus } from '../types/purchaseRequest'

vi.mock('../api/purchaseRequestsApi', () => ({
  purchaseRequestsApi: { getAsBuyer: vi.fn() },
}))

vi.mock('../components/purchase/RateSellerModal', () => ({
  default: ({ onClose, onRated }: { onClose: () => void; onRated: () => void }) => (
    <div data-testid="rate-modal">
      <button onClick={onClose}>Close</button>
      <button onClick={onRated}>Rate</button>
    </div>
  ),
}))

function setup(search = '', state?: Record<string, unknown>) {
  return render(
    <MemoryRouter initialEntries={[{ pathname: '/payment/success', search, state }]}>
      <Routes>
        <Route path="/payment/success" element={<PaymentSuccessPage />} />
      </Routes>
    </MemoryRouter>
  )
}

beforeEach(() => {
  vi.mocked(purchaseRequestsApi.getAsBuyer).mockResolvedValue({ data: [] } as never)
})

describe('PaymentSuccessPage', () => {
  it('renders success title', async () => {
    setup()
    await waitFor(() => expect(screen.getByText('payment.success_title')).toBeInTheDocument())
  })

  it('shows test mode banner when session_id starts with test_', async () => {
    setup('?session_id=test_simulated')
    await waitFor(() => expect(screen.getByText(/Test mode/)).toBeInTheDocument())
  })

  it('does not show test mode banner for real session id', async () => {
    setup('?session_id=cs_live_abc123')
    await waitFor(() => expect(screen.queryByText(/Test mode/)).toBeNull())
  })

  it('shows dashboard and browse links', async () => {
    setup()
    await waitFor(() => {
      expect(screen.getByText('nav.dashboard')).toBeInTheDocument()
      expect(screen.getByText('nav.browse')).toBeInTheDocument()
    })
  })

  it('opens rate modal when completed request found via itemId query param', async () => {
    vi.mocked(purchaseRequestsApi.getAsBuyer).mockResolvedValue({
      data: [
        {
          id: 'req-1',
          itemId: 'item-abc',
          status: PurchaseRequestStatus.Completed,
          sellerDisplayName: 'Seller',
        },
      ],
    } as never)

    setup('?itemId=item-abc')

    await waitFor(() => expect(screen.getByTestId('rate-modal')).toBeInTheDocument())
  })

  it('opens rate modal when completed request found via router state itemId', async () => {
    vi.mocked(purchaseRequestsApi.getAsBuyer).mockResolvedValue({
      data: [
        {
          id: 'req-1',
          itemId: 'item-xyz',
          status: PurchaseRequestStatus.Completed,
          sellerDisplayName: 'Seller',
        },
      ],
    } as never)

    setup('', { itemId: 'item-xyz' })

    await waitFor(() => expect(screen.getByTestId('rate-modal')).toBeInTheDocument())
  })

  it('does not open rate modal when no matching completed request', async () => {
    vi.mocked(purchaseRequestsApi.getAsBuyer).mockResolvedValue({
      data: [
        {
          id: 'req-1',
          itemId: 'item-other',
          status: PurchaseRequestStatus.Completed,
        },
      ],
    } as never)

    setup('?itemId=item-abc')

    await waitFor(() => expect(purchaseRequestsApi.getAsBuyer).toHaveBeenCalled())
    expect(screen.queryByTestId('rate-modal')).toBeNull()
  })

  it('shows rate seller button when modal closed without rating', async () => {
    vi.mocked(purchaseRequestsApi.getAsBuyer).mockResolvedValue({
      data: [
        {
          id: 'req-1',
          itemId: 'item-abc',
          status: PurchaseRequestStatus.Completed,
        },
      ],
    } as never)

    setup('?itemId=item-abc')

    await waitFor(() => expect(screen.getByTestId('rate-modal')).toBeInTheDocument())
    await userEvent.click(screen.getByText('Close'))

    // Button text includes emoji prefix — match by partial text
    await waitFor(() => expect(screen.getByText(/rating\.rate_seller/)).toBeInTheDocument())
  })
})
