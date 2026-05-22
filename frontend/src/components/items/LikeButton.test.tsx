import { describe, it, expect, vi } from 'vitest'
import { render, screen, act, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LikeButton from './LikeButton'

describe('LikeButton', () => {
  it('renders like count', () => {
    render(<LikeButton itemId="1" likeCount={5} isLiked={false} />)
    expect(screen.getByText('5')).toBeInTheDocument()
  })

  it('toggles like on click and updates count', async () => {
    render(<LikeButton itemId="1" likeCount={5} isLiked={false} />)
    await userEvent.click(screen.getByRole('button'))
    expect(screen.getByText('6')).toBeInTheDocument()
  })

  it('decrements count when already liked', async () => {
    render(<LikeButton itemId="1" likeCount={5} isLiked />)
    await userEvent.click(screen.getByRole('button'))
    expect(screen.getByText('4')).toBeInTheDocument()
  })

  it('calls onToggle with itemId', async () => {
    const onToggle = vi.fn()
    render(<LikeButton itemId="item-42" likeCount={0} isLiked={false} onToggle={onToggle} />)
    await userEvent.click(screen.getByRole('button'))
    expect(onToggle).toHaveBeenCalledWith('item-42')
  })

  it('calls onRequireAuth instead of toggling when provided', async () => {
    const onRequireAuth = vi.fn()
    const onToggle = vi.fn()
    render(<LikeButton itemId="1" likeCount={0} isLiked={false} onRequireAuth={onRequireAuth} onToggle={onToggle} />)
    await userEvent.click(screen.getByRole('button'))
    expect(onRequireAuth).toHaveBeenCalledOnce()
    expect(onToggle).not.toHaveBeenCalled()
    expect(screen.getByText('0')).toBeInTheDocument()
  })

  it('renders with size sm (small icon classes)', () => {
    const { container } = render(<LikeButton itemId="1" likeCount={3} isLiked={false} size="sm" />)
    const svg = container.querySelector('svg')
    expect(svg?.getAttribute('class')).toContain('h-4')
  })

  it('renders liked state with size sm', () => {
    const { container } = render(<LikeButton itemId="1" likeCount={3} isLiked size="sm" />)
    const svg = container.querySelector('svg')
    expect(svg?.getAttribute('class')).toContain('h-4')
  })

  it('removes animate-like-bounce class after 400ms', () => {
    vi.useFakeTimers()
    try {
      const { container } = render(<LikeButton itemId="1" likeCount={0} isLiked={false} />)
      fireEvent.click(screen.getByRole('button'))
      const svg = container.querySelector('svg')
      expect(svg?.getAttribute('class')).toContain('animate-like-bounce')
      act(() => { vi.advanceTimersByTime(400) })
      expect(svg?.getAttribute('class')).not.toContain('animate-like-bounce')
    } finally {
      vi.useRealTimers()
    }
  })
})
