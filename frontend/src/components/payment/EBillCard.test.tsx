import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import EBillCard from './EBillCard'
import { ebillsApi } from '../../api/ebillsApi'
import type { EBill } from '../../types/ebill'
import { PaymentMethod } from '../../types/payment'

vi.mock('../../api/ebillsApi', () => ({
  ebillsApi: { resendEmail: vi.fn() },
}))
vi.mock('@/utils/toast', () => ({
  default: { success: vi.fn(), error: vi.fn() },
}))

const mockResendEmail = vi.mocked(ebillsApi.resendEmail)

const bill: EBill = {
  id: 'bill-1',
  eBillNumber: 'INV-001',
  itemId: 'item-1',
  itemTitle: 'Baby Clothes',
  buyerId: 'buyer-1',
  sellerId: 'seller-1',
  sellerDisplayName: 'Jane Seller',
  amount: 19.56,
  currency: 'BGN',
  paymentMethod: PaymentMethod.Card,
  issuedAt: '2024-01-15T10:00:00Z',
  receiptUrl: 'https://example.com/receipt.pdf',
}

beforeEach(() => {
  mockResendEmail.mockClear()
})

describe('EBillCard', () => {
  it('renders bill number', () => {
    render(<EBillCard bill={bill} />)
    expect(screen.getByText('INV-001')).toBeInTheDocument()
  })

  it('renders item title', () => {
    render(<EBillCard bill={bill} />)
    expect(screen.getByText('Baby Clothes')).toBeInTheDocument()
  })

  it('renders seller name', () => {
    render(<EBillCard bill={bill} />)
    expect(screen.getByText('Jane Seller')).toBeInTheDocument()
  })

  it('renders formatted amount', () => {
    render(<EBillCard bill={bill} />)
    expect(screen.getByText(/€10\.00/)).toBeInTheDocument()
  })

  it('renders download link when receiptUrl present', () => {
    render(<EBillCard bill={bill} />)
    expect(screen.getByRole('link', { name: /ebill\.download/ })).toHaveAttribute('href', 'https://example.com/receipt.pdf')
  })

  it('calls resendEmail on resend button click', async () => {
    mockResendEmail.mockResolvedValue({} as never)
    render(<EBillCard bill={bill} />)
    await userEvent.click(screen.getByTitle('ebill.resend'))
    expect(mockResendEmail).toHaveBeenCalledWith('bill-1')
  })

  it('shows error toast when resendEmail rejects', async () => {
    const toast = await import('@/utils/toast')
    mockResendEmail.mockRejectedValue(new Error('Network error'))
    render(<EBillCard bill={bill} />)
    await userEvent.click(screen.getByTitle('ebill.resend'))
    await waitFor(() => expect(vi.mocked(toast.default.error)).toHaveBeenCalledWith('common.error'))
  })

  it('shows disabled spinner text while resending', async () => {
    let resolve!: () => void
    mockResendEmail.mockReturnValue(new Promise<void>((r) => { resolve = r }) as never)
    render(<EBillCard bill={bill} />)
    await userEvent.click(screen.getByTitle('ebill.resend'))
    expect(screen.getByText('…')).toBeInTheDocument()
    expect(screen.getByTitle('ebill.resend')).toBeDisabled()
    resolve()
  })

  it('renders disabled download span when receiptUrl is null', () => {
    render(<EBillCard bill={{ ...bill, receiptUrl: null }} />)
    expect(screen.queryByRole('link', { name: /ebill\.download/ })).toBeNull()
    expect(screen.getByTitle('ebill.download')).toBeInTheDocument()
  })

  it('shows fallback dash when eBillNumber is null', () => {
    render(<EBillCard bill={{ ...bill, eBillNumber: null }} />)
    expect(screen.getByText('—')).toBeInTheDocument()
  })

  it('shows error text when itemTitle is null', () => {
    render(<EBillCard bill={{ ...bill, itemTitle: null }} />)
    expect(screen.getByText('common.error')).toBeInTheDocument()
  })

  it('uses raw paymentMethod string for non-Card methods', () => {
    render(<EBillCard bill={{ ...bill, paymentMethod: 99 as never }} />)
    expect(screen.getByText('99')).toBeInTheDocument()
  })
})
