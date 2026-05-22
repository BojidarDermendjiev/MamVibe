import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, waitFor, act } from '@testing-library/react'
import CloudflareGate from './CloudflareGate'
import { turnstileApi } from '../../api/turnstileApi'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (k: string) => k }),
}))

vi.mock('../../api/turnstileApi', () => ({
  turnstileApi: { verify: vi.fn() },
}))

const mockVerify = vi.mocked(turnstileApi.verify)
const SESSION_KEY = 'cf_verified'

function mountTurnstile() {
  let onSuccess: ((token: string) => void) | null = null
  let onError: (() => void) | null = null
  const mock = {
    render: vi.fn((_, opts) => {
      onSuccess = opts.callback
      onError = opts['error-callback'] ?? null
      return 'widget-1'
    }),
    reset: vi.fn(),
    remove: vi.fn(),
  }
  Object.defineProperty(window, 'turnstile', { value: mock, writable: true, configurable: true })
  return {
    mock,
    fireSuccess: (token = 'tok') => onSuccess!(token),
    fireError: () => onError?.(),
  }
}

function removeTurnstile() {
  try { delete (window as unknown as Record<string, unknown>).turnstile } catch { /* noop */ }
}

function cleanupScripts() {
  document.querySelectorAll('script[src*="challenges.cloudflare.com"]').forEach((s) => s.remove())
}

beforeEach(() => {
  sessionStorage.clear()
  mockVerify.mockClear()
  removeTurnstile()
  cleanupScripts()
})

afterEach(() => {
  removeTurnstile()
  cleanupScripts()
})

describe('CloudflareGate', () => {
  it('renders children immediately when session is already verified', () => {
    sessionStorage.setItem(SESSION_KEY, String(Date.now() + 60_000))
    render(<CloudflareGate><span>Protected content</span></CloudflareGate>)
    expect(screen.getByText('Protected content')).toBeInTheDocument()
  })

  it('shows verification UI when session is not verified', () => {
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    expect(screen.queryByText('Protected')).not.toBeInTheDocument()
    expect(screen.getByText('turnstile.title')).toBeInTheDocument()
  })

  it('shows loading spinner while turnstile script is loading', () => {
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    expect(document.querySelector('.animate-spin')).toBeInTheDocument()
  })

  it('injects the Turnstile script tag when not already present', () => {
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    expect(document.querySelector('script[src*="challenges.cloudflare.com"]')).toBeInTheDocument()
  })

  it('does not inject a duplicate script if one already exists', () => {
    const existing = document.createElement('script')
    existing.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit'
    document.head.appendChild(existing)
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    expect(document.querySelectorAll('script[src*="challenges.cloudflare.com"]')).toHaveLength(1)
  })

  it('renders the Turnstile widget when window.turnstile is available', async () => {
    const { mock } = mountTurnstile()
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    await waitFor(() => expect(mock.render).toHaveBeenCalled())
  })

  it('shows children after successful token verification', async () => {
    const { fireSuccess } = mountTurnstile()
    mockVerify.mockResolvedValue({ data: { verified: true } } as never)
    render(<CloudflareGate><span>Protected content</span></CloudflareGate>)
    await act(async () => { fireSuccess('valid-token') })
    await waitFor(() => expect(screen.getByText('Protected content')).toBeInTheDocument())
  })

  it('stores expiry in sessionStorage after successful verification', async () => {
    const { fireSuccess } = mountTurnstile()
    mockVerify.mockResolvedValue({ data: { verified: true } } as never)
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    await act(async () => { fireSuccess('valid-token') })
    await waitFor(() => expect(sessionStorage.getItem(SESSION_KEY)).not.toBeNull())
  })

  it('shows error and resets widget when verification returns false', async () => {
    const { mock, fireSuccess } = mountTurnstile()
    mockVerify.mockResolvedValue({ data: { verified: false } } as never)
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    await act(async () => { fireSuccess('bad-token') })
    await waitFor(() => expect(screen.getByText('turnstile.error')).toBeInTheDocument())
    expect(mock.reset).toHaveBeenCalledWith('widget-1')
  })

  it('shows error and resets widget when API throws', async () => {
    const { mock, fireSuccess } = mountTurnstile()
    mockVerify.mockRejectedValue(new Error('Network error'))
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    await act(async () => { fireSuccess('token') })
    await waitFor(() => expect(screen.getByText('turnstile.error')).toBeInTheDocument())
    expect(mock.reset).toHaveBeenCalled()
  })

  it('shows error when the widget fires its error-callback', async () => {
    const { fireError } = mountTurnstile()
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    await waitFor(() => {}) // let render effect run
    act(() => { fireError() })
    expect(screen.getByText('turnstile.error')).toBeInTheDocument()
  })

  it('shows verifying indicator while API call is in progress', async () => {
    let resolve!: (v: unknown) => void
    const { fireSuccess } = mountTurnstile()
    mockVerify.mockReturnValue(new Promise((r) => { resolve = r }) as never)
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    act(() => { fireSuccess('token') })
    await waitFor(() => expect(screen.getByText('turnstile.verifying')).toBeInTheDocument())
    await act(async () => { resolve({ data: { verified: true } }) })
  })

  it('removes the widget on unmount', async () => {
    const { mock } = mountTurnstile()
    const { unmount } = render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    await waitFor(() => expect(mock.render).toHaveBeenCalled())
    unmount()
    expect(mock.remove).toHaveBeenCalledWith('widget-1')
  })

  it('treats an expired session timestamp as unverified', () => {
    sessionStorage.setItem(SESSION_KEY, String(Date.now() - 1000))
    render(<CloudflareGate><span>Protected</span></CloudflareGate>)
    expect(screen.queryByText('Protected')).not.toBeInTheDocument()
    expect(screen.getByText('turnstile.title')).toBeInTheDocument()
  })
})
