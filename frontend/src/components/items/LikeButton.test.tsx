import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
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
})
