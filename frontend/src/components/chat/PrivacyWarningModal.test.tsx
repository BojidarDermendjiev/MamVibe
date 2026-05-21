import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PrivacyWarningModal from './PrivacyWarningModal'
import type { SensitiveMatch } from '../../utils/sensitiveDataDetector'

const matches: SensitiveMatch[] = [
  { type: 'phone', labelKey: 'privacy.type_phone' },
  { type: 'email', labelKey: 'privacy.type_email' },
]

describe('PrivacyWarningModal', () => {
  it('renders a badge for each matched type', () => {
    render(<PrivacyWarningModal matches={matches} onEdit={vi.fn()} onSendAnyway={vi.fn()} />)
    expect(screen.getByText('privacy.type_phone')).toBeInTheDocument()
    expect(screen.getByText('privacy.type_email')).toBeInTheDocument()
  })

  it('calls onEdit when edit button is clicked', async () => {
    const onEdit = vi.fn()
    render(<PrivacyWarningModal matches={matches} onEdit={onEdit} onSendAnyway={vi.fn()} />)
    await userEvent.click(screen.getByText('privacy.btn_edit'))
    expect(onEdit).toHaveBeenCalledOnce()
  })

  it('calls onSendAnyway when send anyway is clicked', async () => {
    const onSendAnyway = vi.fn()
    render(<PrivacyWarningModal matches={matches} onEdit={vi.fn()} onSendAnyway={onSendAnyway} />)
    await userEvent.click(screen.getByText('privacy.btn_send_anyway'))
    expect(onSendAnyway).toHaveBeenCalledOnce()
  })

  it('calls onEdit when backdrop is clicked', async () => {
    const onEdit = vi.fn()
    const { container } = render(<PrivacyWarningModal matches={matches} onEdit={onEdit} onSendAnyway={vi.fn()} />)
    const backdrop = container.querySelector('.fixed.inset-0') as HTMLElement
    await userEvent.click(backdrop)
    expect(onEdit).toHaveBeenCalled()
  })
})
