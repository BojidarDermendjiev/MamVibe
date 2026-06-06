import React from 'react'
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import _toast from 'react-hot-toast'
import type { Toast } from 'react-hot-toast'
import toast, { toastSuccess, toastError, toastWarning, toastInfo } from './toast'

vi.mock('lucide-react', async (importOriginal) => {
  const actual = await importOriginal<typeof import('lucide-react')>();
  return {
    ...actual,
    CheckCircle: () => React.createElement('svg', { 'data-testid': 'icon-success' }),
    XCircle: () => React.createElement('svg', { 'data-testid': 'icon-error' }),
    AlertTriangle: () => React.createElement('svg', { 'data-testid': 'icon-warning' }),
    Info: () => React.createElement('svg', { 'data-testid': 'icon-info' }),
    X: () => React.createElement('svg', { 'data-testid': 'icon-x' }),
  };
})

let lastRenderFn: ((t: Toast) => React.ReactElement) | null = null

beforeEach(() => {
  lastRenderFn = null
  vi.spyOn(_toast, 'custom').mockImplementation(((fn: (t: Toast) => React.ReactNode) => {
    lastRenderFn = fn as (t: Toast) => React.ReactElement
    return 'toast-id'
  }) as never)
  vi.spyOn(_toast, 'dismiss')
})

afterEach(() => {
  vi.restoreAllMocks()
})

function fakeToast(overrides: Partial<Toast> = {}): Toast {
  return { id: 'test-id', visible: true, duration: 3500, ...overrides } as Toast
}

function renderCard(overrides: Partial<Toast> = {}) {
  if (!lastRenderFn) throw new Error('No render fn captured — call a toast helper first')
  return render(lastRenderFn(fakeToast(overrides)))
}

describe('ToastCard rendering', () => {
  it('success variant renders the success icon', () => {
    toast.success('Saved!')
    renderCard()
    expect(screen.getByTestId('icon-success')).toBeInTheDocument()
  })

  it('success variant renders the title', () => {
    toast.success('Saved!')
    renderCard()
    expect(screen.getByText('Saved!')).toBeInTheDocument()
  })

  it('error variant renders the error icon', () => {
    toast.error('Failed!')
    renderCard()
    expect(screen.getByTestId('icon-error')).toBeInTheDocument()
  })

  it('warning variant renders the warning icon', () => {
    toast.warning('Watch out!')
    renderCard()
    expect(screen.getByTestId('icon-warning')).toBeInTheDocument()
  })

  it('info variant renders the info icon', () => {
    toast.info('FYI')
    renderCard()
    expect(screen.getByTestId('icon-info')).toBeInTheDocument()
  })

  it('renders the optional subtitle message', () => {
    toastSuccess('Title', 'Details here')
    renderCard()
    expect(screen.getByText('Details here')).toBeInTheDocument()
  })

  it('omits the message paragraph when subtitle is absent', () => {
    toast.success('Title only')
    renderCard()
    expect(screen.queryByText('Details here')).not.toBeInTheDocument()
  })

  it('dismiss button calls _toast.dismiss with the toast id', async () => {
    toast.success('Hi')
    renderCard({ id: 'my-toast' })
    await userEvent.click(screen.getByLabelText('Dismiss'))
    expect(_toast.dismiss).toHaveBeenCalledWith('my-toast')
  })

  it('applies opacity-100 when the toast is visible', () => {
    toast.success('Visible')
    const { container } = renderCard({ visible: true })
    expect(container.querySelector('[role="alert"]')).toHaveClass('opacity-100')
  })

  it('applies opacity-0 when the toast is not visible', () => {
    toast.success('Hidden')
    const { container } = renderCard({ visible: false })
    expect(container.querySelector('[role="alert"]')).toHaveClass('opacity-0')
  })

  it('renders the progress bar element', () => {
    toast.success('Progress')
    const { container } = renderCard()
    expect(container.querySelector('.toast-progress')).toBeInTheDocument()
  })
})

describe('toast helper methods', () => {
  it('toast.success calls _toast.custom', () => {
    toast.success('OK')
    expect(_toast.custom).toHaveBeenCalledWith(
      expect.any(Function),
      expect.objectContaining({ duration: 3500 }),
    )
  })

  it('toast.error calls _toast.custom', () => {
    toast.error('Oops')
    expect(_toast.custom).toHaveBeenCalled()
  })

  it('toast.warning calls _toast.custom', () => {
    toast.warning('Careful')
    expect(_toast.custom).toHaveBeenCalled()
  })

  it('toast.info calls _toast.custom', () => {
    toast.info('FYI')
    expect(_toast.custom).toHaveBeenCalled()
  })
})

describe('named helper exports', () => {
  it('toastSuccess passes title and subtitle to the card', () => {
    toastSuccess('Good news', 'More details')
    renderCard()
    expect(screen.getByText('Good news')).toBeInTheDocument()
    expect(screen.getByText('More details')).toBeInTheDocument()
  })

  it('toastError calls _toast.custom', () => {
    toastError('Bad news', 'Error details')
    expect(_toast.custom).toHaveBeenCalled()
  })

  it('toastWarning calls _toast.custom', () => {
    toastWarning('Heads up')
    expect(_toast.custom).toHaveBeenCalled()
  })

  it('toastInfo calls _toast.custom', () => {
    toastInfo('For your info')
    expect(_toast.custom).toHaveBeenCalled()
  })
})
