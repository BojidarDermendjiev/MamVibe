import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import FeedbackCard from './FeedbackCard'
import { FeedbackCategory } from '../../types/feedback'
import type { Feedback } from '../../types/feedback'

const baseFeedback: Feedback = {
  id: 'fb-1',
  userId: 'u-1',
  userDisplayName: 'Test User',
  userAvatarUrl: null,
  rating: 4,
  category: FeedbackCategory.Praise,
  content: 'Great platform!',
  isContactable: false,
  createdAt: '2024-01-15T10:00:00Z',
}

describe('FeedbackCard', () => {
  it('renders display name', () => {
    render(<FeedbackCard feedback={baseFeedback} />)
    expect(screen.getByText('Test User')).toBeInTheDocument()
  })

  it('renders content', () => {
    render(<FeedbackCard feedback={baseFeedback} />)
    expect(screen.getByText('Great platform!')).toBeInTheDocument()
  })

  it('renders formatted date', () => {
    render(<FeedbackCard feedback={baseFeedback} />)
    expect(screen.getByText(new Date('2024-01-15T10:00:00Z').toLocaleDateString())).toBeInTheDocument()
  })

  it('does not show delete button when canDelete is false', () => {
    render(<FeedbackCard feedback={baseFeedback} canDelete={false} onDelete={vi.fn()} />)
    expect(screen.queryByText('feedback.delete')).toBeNull()
  })

  it('shows delete button when canDelete and onDelete provided', () => {
    render(<FeedbackCard feedback={baseFeedback} canDelete onDelete={vi.fn()} />)
    expect(screen.getByText('feedback.delete')).toBeInTheDocument()
  })

  it('calls onDelete with feedback id when delete clicked', async () => {
    const onDelete = vi.fn()
    render(<FeedbackCard feedback={baseFeedback} canDelete onDelete={onDelete} />)
    await userEvent.click(screen.getByText('feedback.delete'))
    expect(onDelete).toHaveBeenCalledWith('fb-1')
  })

  it('renders avatar img when userAvatarUrl is provided', () => {
    const feedback = { ...baseFeedback, userAvatarUrl: '/avatar.jpg' }
    const { container } = render(<FeedbackCard feedback={feedback} />)
    expect(container.querySelector('img[src="/avatar.jpg"]')).toBeInTheDocument()
  })

  it('renders initials fallback when no avatar', () => {
    render(<FeedbackCard feedback={baseFeedback} />)
    expect(screen.getByText('T')).toBeInTheDocument()
  })
})
