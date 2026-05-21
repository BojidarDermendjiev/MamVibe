import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import NekorektenWarningModal from './NekorektenWarningModal'

const baseProps = {
  isOpen: true,
  onClose: vi.fn(),
  onConfirm: vi.fn(),
  sellerName: 'Ivan Ivanov',
  reportUrl: 'https://nekorekten.com/ivan',
}

describe('NekorektenWarningModal', () => {
  it('renders seller name', () => {
    render(<NekorektenWarningModal {...baseProps} />)
    expect(screen.getByText('Ivan Ivanov')).toBeInTheDocument()
  })

  it('renders report link with correct href', () => {
    render(<NekorektenWarningModal {...baseProps} />)
    expect(screen.getByRole('link')).toHaveAttribute('href', 'https://nekorekten.com/ivan')
  })

  it('calls onClose when cancel button clicked', async () => {
    const onClose = vi.fn()
    render(<NekorektenWarningModal {...baseProps} onClose={onClose} />)
    await userEvent.click(screen.getByText('common.cancel'))
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onConfirm when continue button clicked', async () => {
    const onConfirm = vi.fn()
    render(<NekorektenWarningModal {...baseProps} onConfirm={onConfirm} />)
    await userEvent.click(screen.getByText('nekorekten.continue_anyway'))
    expect(onConfirm).toHaveBeenCalledOnce()
  })

  it('renders nothing when closed', () => {
    const { container } = render(<NekorektenWarningModal {...baseProps} isOpen={false} />)
    expect(container.firstChild).toBeNull()
  })
})
