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

  it('shows error toast when API call fails', async () => {
    const toast = await import('@/utils/toast')
    mockCreate.mockRejectedValue(new Error('Server error'))
    render(<RateSellerModal {...baseProps} />)
    await userEvent.click(screen.getAllByRole('button')[5])
    await userEvent.click(screen.getByText('rating.submit'))
    await waitFor(() => expect(vi.mocked(toast.default.error)).toHaveBeenCalledWith('common.error'))
  })

  it('does not show seller name paragraph when sellerName is null', () => {
    render(<RateSellerModal {...baseProps} sellerName={null} />)
    expect(screen.queryByText(/rating\.rating_for/)).toBeNull()
  })

  it('passes comment to API when provided', async () => {
    mockCreate.mockResolvedValue({} as never)
    render(<RateSellerModal {...baseProps} />)
    await userEvent.click(screen.getAllByRole('button')[5])
    const textarea = screen.getByPlaceholderText('rating.comment_placeholder')
    await userEvent.type(textarea, 'Great seller!')
    await userEvent.click(screen.getByText('rating.submit'))
    await waitFor(() => expect(mockCreate).toHaveBeenCalledWith('pr-1', { rating: 5, comment: 'Great seller!' }))
  })

  it('shows rating.star_1 label when 1 star selected', async () => {
    render(<RateSellerModal {...baseProps} />)
    await userEvent.click(screen.getAllByRole('button')[1])
    expect(screen.getByText('rating.star_1')).toBeInTheDocument()
  })

  it('shows rating.star_3 label when 3 stars selected', async () => {
    render(<RateSellerModal {...baseProps} />)
    await userEvent.click(screen.getAllByRole('button')[3])
    expect(screen.getByText('rating.star_3')).toBeInTheDocument()
  })
})
