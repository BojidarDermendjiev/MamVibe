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

  it('uses singular "report" when reportCount is 1', () => {
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={{ ...result, reportCount: 1 }} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.getByText('1 report found')).toBeInTheDocument()
  })

  it('shows fallback text when report has no text', () => {
    const noText = { ...result, reports: [{ text: '', createdAt: undefined, likes: 0 }] }
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={noText} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.getByText('No description provided.')).toBeInTheDocument()
  })

  it('does not render date when createdAt is null', () => {
    const noDate = { ...result, reports: [{ text: 'Some report', createdAt: undefined, likes: 0 }] }
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={noDate} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.queryByText(/\d{1,2}\/\d{1,2}\/\d{4}/)).toBeNull()
  })

  it('does not render likes when likes is 0', () => {
    const noLikes = { ...result, reports: [{ text: 'Report', createdAt: '2024-01-01T00:00:00Z', likes: 0 }] }
    const { container } = render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={noLikes} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(container.querySelector('svg[data-icon]')).toBeNull()
  })

  it('renders no reports list when reports array is empty', () => {
    const emptyReports = { ...result, reports: [] }
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={emptyReports} onAccept={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.queryByText('Did not pay')).toBeNull()
  })

  it('calls onCancel when X button in header clicked', async () => {
    const onCancel = vi.fn()
    render(<BuyerReputationModal buyerName="John" buyerAvatarUrl={null} result={result} onAccept={vi.fn()} onCancel={onCancel} />)
    await userEvent.click(screen.getAllByRole('button')[0])
    expect(onCancel).toHaveBeenCalledOnce()
  })
})
