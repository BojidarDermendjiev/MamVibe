import { describe, it, expect, vi } from 'vitest'
import { render, screen, act, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Button from './Button'

describe('Button', () => {
  it('renders children', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByRole('button', { name: 'Click me' })).toBeInTheDocument()
  })

  it('calls onClick when clicked', async () => {
    const handler = vi.fn()
    render(<Button onClick={handler}>Go</Button>)
    await userEvent.click(screen.getByRole('button'))
    expect(handler).toHaveBeenCalledOnce()
  })

  it('is disabled when disabled prop is set', () => {
    render(<Button disabled>Save</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('is disabled and shows spinner when isLoading', () => {
    render(<Button isLoading>Save</Button>)
    const btn = screen.getByRole('button')
    expect(btn).toBeDisabled()
    expect(btn.querySelector('svg')).toBeInTheDocument()
  })

  it('does not call onClick when disabled', async () => {
    const handler = vi.fn()
    render(<Button disabled onClick={handler}>Save</Button>)
    await userEvent.click(screen.getByRole('button'))
    expect(handler).not.toHaveBeenCalled()
  })

  it('applies w-full class when fullWidth', () => {
    render(<Button fullWidth>Wide</Button>)
    expect(screen.getByRole('button')).toHaveClass('w-full')
  })

  it('forwards extra HTML attributes', () => {
    render(<Button data-testid="my-btn" type="submit">Submit</Button>)
    expect(screen.getByTestId('my-btn')).toHaveAttribute('type', 'submit')
  })

  it('adds ripple span on click', () => {
    render(<Button>Go</Button>)
    fireEvent.click(screen.getByRole('button'))
    expect(document.querySelector('.animate-rippling')).toBeInTheDocument()
  })

  it('removes ripple span after 600ms', () => {
    vi.useFakeTimers()
    try {
      render(<Button>Go</Button>)
      fireEvent.click(screen.getByRole('button'))
      expect(document.querySelector('.animate-rippling')).toBeInTheDocument()
      act(() => { vi.advanceTimersByTime(600) })
      expect(document.querySelector('.animate-rippling')).toBeNull()
    } finally {
      vi.useRealTimers()
    }
  })

  it('cancels ripple timeout on unmount', () => {
    vi.useFakeTimers()
    try {
      const { unmount } = render(<Button>Go</Button>)
      fireEvent.click(screen.getByRole('button'))
      unmount()
      act(() => { vi.advanceTimersByTime(600) })
    } finally {
      vi.useRealTimers()
    }
  })
})
