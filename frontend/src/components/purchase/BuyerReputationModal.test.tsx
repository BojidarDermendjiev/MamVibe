import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BuyerReputationModal from './BuyerReputationModal'
import type { BuyerCheckResult } from '../../api/purchaseRequestsApi'

const result: BuyerCheckResult = {
  hasReports: true,
  reportCount: 2,
  serviceUnavailable: false,
  reports: [
    { text: 'Did not pay', createdAt: '2024-01-01T00:00:00Z', likes: 3 },
  ],
}

describe('BuyerReputationModal', () => {
  it('renders buyer name', () => {
    render(<BuyerReputationModal buyerName="John Doe" buyerAvatarUrl={null} result={result} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.getByText('John Doe')).toBeInTheDocument()
  })

  it('renders report count', () => {
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={result} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.getByText('2 reports found')).toBeInTheDocument()
  })

  it('renders report text', () => {
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={result} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.getByText('Did not pay')).toBeInTheDocument()
  })

  it('calls onAccept when Accept Anyway clicked', async () => {
    const onAccept = vi.fn()
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={result} onAccept={onAccept} onCancel={vi.fn()} />)
    await userEvent.click(screen.getByText('Accept Anyway'))
    expect(onAccept).toHaveBeenCalledOnce()
  })

  it('calls onCancel when Cancel clicked', async () => {
    const onCancel = vi.fn()
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={result} onAccept={vi.fn()} onCancel={onCancel} />)
    await userEvent.click(screen.getByText('Cancel'))
    expect(onCancel).toHaveBeenCalledOnce()
  })

  it('renders fallback name when buyerName is null', () => {
    render(<BuyerReputationModal buyerName={null} buyerAvatarUrl={null} result={result} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.getByText('Unknown Buyer')).toBeInTheDocument()
  })
})
