import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import CookieConsent from './CookieConsent'

function renderComponent() {
  return render(
    <MemoryRouter>
      <CookieConsent />
    </MemoryRouter>
  )
}

beforeEach(() => {
  localStorage.clear()
})

describe('CookieConsent', () => {
  it('shows settings modal when no consent stored', () => {
    renderComponent()
    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })

  it('always renders the floating FAB', () => {
    localStorage.setItem('cookieConsent', 'customized')
    renderComponent()
    // i18n returns the key in test env; aria-label = t('common.cookie_settings')
    expect(screen.getByRole('button', { name: 'common.cookie_settings' })).toBeInTheDocument()
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('opens settings modal when FAB is clicked', async () => {
    localStorage.setItem('cookieConsent', 'customized')
    renderComponent()
    await userEvent.click(screen.getByRole('button', { name: 'common.cookie_settings' }))
    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })

  it('saves preferences and hides modal on Save & Exit click', async () => {
    renderComponent()
    await userEvent.click(screen.getByText('common.cookie_save_exit'))
    expect(localStorage.getItem('cookieConsent')).toBe('customized')
    expect(localStorage.getItem('cookiePreferences')).not.toBeNull()
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('saves and closes on X button click', async () => {
    renderComponent()
    await userEvent.click(screen.getByRole('button', { name: 'Close' }))
    expect(localStorage.getItem('cookieConsent')).toBe('customized')
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('toggles accordion sections on + click', async () => {
    renderComponent()
    const functionalRow = screen.getByText('common.cookie_functional')
    await userEvent.click(functionalRow)
    expect(screen.getByText('common.cookie_functional_desc')).toBeInTheDocument()
    await userEvent.click(functionalRow)
    expect(screen.queryByText('common.cookie_functional_desc')).toBeNull()
  })
})
