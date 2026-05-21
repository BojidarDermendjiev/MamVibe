import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import StarRating from './StarRating'

describe('feedback/StarRating', () => {
  it('renders 5 star buttons', () => {
    render(<StarRating value={0} />)
    expect(screen.getAllByRole('button')).toHaveLength(5)
  })

  it('calls onChange with clicked star value', async () => {
    const onChange = vi.fn()
    render(<StarRating value={0} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[3])
    expect(onChange).toHaveBeenCalledWith(4)
  })

  it('does not call onChange when readonly', async () => {
    const onChange = vi.fn()
    render(<StarRating value={3} onChange={onChange} readonly />)
    await userEvent.click(screen.getAllByRole('button')[0])
    expect(onChange).not.toHaveBeenCalled()
  })

  it('disables all buttons when readonly', () => {
    render(<StarRating value={3} readonly />)
    screen.getAllByRole('button').forEach((btn) => expect(btn).toBeDisabled())
  })
})
