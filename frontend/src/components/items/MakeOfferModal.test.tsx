import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MakeOfferModal from './MakeOfferModal'
import { offersApi } from '../../api/offersApi'
import toast from '../../utils/toast'

vi.mock('../../api/offersApi', () => ({ offersApi: { create: vi.fn() } }))
vi.mock('../../utils/toast', () => ({ default: { error: vi.fn(), success: vi.fn() } }))
vi.mock('../common/Modal', () => ({
  default: ({ children, title }: { children: React.ReactNode; title: string }) => (
    <div>
      <h2>{title}</h2>
      {children}
    </div>
  ),
}))

const baseProps = {
  itemId: 'item-1',
  itemTitle: 'Baby Stroller',
  listingPrice: 100,
  onClose: vi.fn(),
  onSuccess: vi.fn(),
}

beforeEach(() => {
  vi.mocked(offersApi.create).mockClear()
  vi.mocked(toast.error).mockClear()
  vi.mocked(toast.success).mockClear()
  baseProps.onClose = vi.fn()
  baseProps.onSuccess = vi.fn()
})

describe('MakeOfferModal', () => {
  it('renders modal title', () => {
    render(<MakeOfferModal {...baseProps} />)
    expect(screen.getByText('offer.title')).toBeInTheDocument()
  })

  it('renders item title and listing price', () => {
    render(<MakeOfferModal {...baseProps} />)
    expect(screen.getByText('Baby Stroller')).toBeInTheDocument()
  })

  it('renders price input', () => {
    render(<MakeOfferModal {...baseProps} />)
    expect(screen.getByRole('spinbutton')).toBeInTheDocument()
  })

  it('calls offersApi.create with correct args on valid submit', async () => {
    vi.mocked(offersApi.create).mockResolvedValue({ data: {} } as never)
    render(<MakeOfferModal {...baseProps} />)

    await userEvent.type(screen.getByRole('spinbutton'), '75')
    await userEvent.click(screen.getByText('offer.send'))

    await waitFor(() => expect(offersApi.create).toHaveBeenCalledWith('item-1', 75))
  })

  it('calls onSuccess after successful submission', async () => {
    vi.mocked(offersApi.create).mockResolvedValue({ data: {} } as never)
    render(<MakeOfferModal {...baseProps} />)

    await userEvent.type(screen.getByRole('spinbutton'), '75')
    await userEvent.click(screen.getByText('offer.send'))

    await waitFor(() => expect(baseProps.onSuccess).toHaveBeenCalled())
  })

  it('shows success toast after submission', async () => {
    vi.mocked(offersApi.create).mockResolvedValue({ data: {} } as never)
    render(<MakeOfferModal {...baseProps} />)

    await userEvent.type(screen.getByRole('spinbutton'), '75')
    await userEvent.click(screen.getByText('offer.send'))

    await waitFor(() => expect(toast.success).toHaveBeenCalledWith('offer.sent'))
  })

  it('shows error toast when api call fails', async () => {
    vi.mocked(offersApi.create).mockRejectedValue({ response: { data: { error: 'Already have an offer' } } })
    render(<MakeOfferModal {...baseProps} />)

    await userEvent.type(screen.getByRole('spinbutton'), '75')
    await userEvent.click(screen.getByText('offer.send'))

    await waitFor(() => expect(toast.error).toHaveBeenCalledWith('Already have an offer'))
  })

  it('shows fallback error toast when api error has no message', async () => {
    vi.mocked(offersApi.create).mockRejectedValue(new Error('network'))
    render(<MakeOfferModal {...baseProps} />)

    await userEvent.type(screen.getByRole('spinbutton'), '75')
    await userEvent.click(screen.getByText('offer.send'))

    await waitFor(() => expect(toast.error).toHaveBeenCalledWith('offer.send_error'))
  })

  it('calls onClose when cancel button clicked', async () => {
    render(<MakeOfferModal {...baseProps} />)
    await userEvent.click(screen.getByText('common.cancel'))
    expect(baseProps.onClose).toHaveBeenCalled()
  })

  it('shows error toast for invalid price (zero)', async () => {
    render(<MakeOfferModal {...baseProps} />)
    await userEvent.type(screen.getByRole('spinbutton'), '0')
    await userEvent.click(screen.getByText('offer.send'))
    await waitFor(() => expect(toast.error).toHaveBeenCalledWith('offer.invalid_price'))
    expect(offersApi.create).not.toHaveBeenCalled()
  })
})
