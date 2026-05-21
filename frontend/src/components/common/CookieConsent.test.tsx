import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import CookieConsent from './CookieConsent'

beforeEach(() => {
  localStorage.clear()
})

describe('CookieConsent', () => {
  it('renders when no consent stored', () => {
    render(<CookieConsent />)
    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })

  it('renders nothing when consent already stored', () => {
    localStorage.setItem('cookieConsent', 'accepted')
    const { container } = render(<CookieConsent />)
    expect(container.firstChild).toBeNull()
  })

  it('hides and stores accepted on accept click', async () => {
    render(<CookieConsent />)
    await userEvent.click(screen.getByText('common.cookie_accept'))
    expect(localStorage.getItem('cookieConsent')).toBe('accepted')
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('hides and stores rejected on reject click', async () => {
    render(<CookieConsent />)
    await userEvent.click(screen.getByText('common.cookie_reject'))
    expect(localStorage.getItem('cookieConsent')).toBe('rejected')
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('hides on close button (X) click', async () => {
    render(<CookieConsent />)
    await userEvent.click(screen.getByRole('button', { name: 'Close' }))
    expect(screen.queryByRole('dialog')).toBeNull()
  })
})
