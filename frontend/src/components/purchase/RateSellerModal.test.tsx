import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import RateSellerModal from './RateSellerModal'
import { userRatingsApi } from '../../api/userRatingsApi'

vi.mock('../../api/userRatingsApi', () => ({
  userRatingsApi: { create: vi.fn() },
}))
vi.mock('@/utils/toast', () => ({
  default: { success: vi.fn(), error: vi.fn() },
}))

const mockCreate = vi.mocked(userRatingsApi.create)

const baseProps = {
  purchaseRequestId: 'pr-1',
  sellerName: 'Ivan Seller',
  onClose: vi.fn(),
  onRated: vi.fn(),
}

beforeEach(() => {
  mockCreate.mockClear()
  baseProps.onClose.mockClear()
  baseProps.onRated.mockClear()
})

describe('RateSellerModal', () => {
  it('renders seller name', () => {
    render(<RateSellerModal {...baseProps} />)
    expect(screen.getByText('Ivan Seller')).toBeInTheDocument()
  })

  it('calls onClose when Cancel clicked', async () => {
    render(<RateSellerModal {...baseProps} />)
    await userEvent.click(screen.getByText('common.cancel'))
    expect(baseProps.onClose).toHaveBeenCalledOnce()
  })

  it('submit button is disabled when no stars selected', () => {
    render(<RateSellerModal {...baseProps} />)
    expect(screen.getByText('rating.submit')).toBeDisabled()
  })

  it('enables submit after selecting a star', async () => {
    render(<RateSellerModal {...baseProps} />)
    await userEvent.click(screen.getAllByRole('button')[3]) // 4th star
    expect(screen.getByText('rating.submit')).not.toBeDisabled()
  })

  it('calls API and onRated on successful submit', async () => {
    mockCreate.mockResolvedValue({} as never)
    render(<RateSellerModal {...baseProps} />)
    await userEvent.click(screen.getAllByRole('button')[5]) // 5th star (index 0 = X close btn)
    await userEvent.click(screen.getByText('rating.submit'))
    await waitFor(() => {
      expect(mockCreate).toHaveBeenCalledWith('pr-1', { rating: 5, comment: undefined })
      expect(baseProps.onRated).toHaveBeenCalled()
      expect(baseProps.onClose).toHaveBeenCalled()
    })
  })
})
