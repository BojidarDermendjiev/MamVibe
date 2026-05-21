import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import StarRating from './StarRating'

describe('StarRating', () => {
  it('renders 5 star buttons', () => {
    render(<StarRating value={0} />)
    expect(screen.getAllByRole('button')).toHaveLength(5)
  })

  it('calls onChange with the clicked star value', async () => {
    const handler = vi.fn()
    render(<StarRating value={0} onChange={handler} />)
    await userEvent.click(screen.getAllByRole('button')[2]) // 3rd star → value 3
    expect(handler).toHaveBeenCalledWith(3)
  })

  it('does not call onChange when readonly', async () => {
    const handler = vi.fn()
    render(<StarRating value={3} onChange={handler} readonly />)
    await userEvent.click(screen.getAllByRole('button')[0])
    expect(handler).not.toHaveBeenCalled()
  })

  it('disables all buttons when readonly', () => {
    render(<StarRating value={3} readonly />)
    screen.getAllByRole('button').forEach((btn) => {
      expect(btn).toBeDisabled()
    })
  })

  it('does not disable buttons when not readonly', () => {
    render(<StarRating value={3} />)
    screen.getAllByRole('button').forEach((btn) => {
      expect(btn).not.toBeDisabled()
    })
  })
})
