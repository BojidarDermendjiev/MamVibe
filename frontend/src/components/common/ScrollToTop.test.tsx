import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'

import ScrollToTop from './ScrollToTop'

beforeEach(() => {
  Object.defineProperty(window, 'scrollY', { value: 0, configurable: true, writable: true })
  window.scrollTo = vi.fn()
})

afterEach(() => {
  vi.restoreAllMocks()
})

describe('ScrollToTop', () => {
  it('does not render button when scrollY is 0', () => {
    render(<ScrollToTop />)
    expect(screen.queryByRole('button', { name: 'Scroll to top' })).toBeNull()
  })

  it('renders button after scrolling past 300px', () => {
    render(<ScrollToTop />)
    act(() => {
      Object.defineProperty(window, 'scrollY', { value: 400, configurable: true })
      window.dispatchEvent(new Event('scroll'))
    })
    expect(screen.getByRole('button', { name: 'Scroll to top' })).toBeInTheDocument()
  })

  it('calls window.scrollTo when button is clicked', async () => {
    render(<ScrollToTop />)
    act(() => {
      Object.defineProperty(window, 'scrollY', { value: 400, configurable: true })
      window.dispatchEvent(new Event('scroll'))
    })
    await userEvent.click(screen.getByRole('button', { name: 'Scroll to top' }))
    expect(window.scrollTo).toHaveBeenCalledWith({ top: 0, behavior: 'smooth' })
  })
})
