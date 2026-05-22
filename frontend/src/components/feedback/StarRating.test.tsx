import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
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

  it('highlights stars up to hovered position on mouseEnter', () => {
    const { container } = render(<StarRating value={0} />)
    fireEvent.mouseEnter(screen.getAllByRole('button')[2]) // star 3
    const svgs = container.querySelectorAll('svg')
    expect(svgs[0].getAttribute('class')).toContain('text-peach')
    expect(svgs[2].getAttribute('class')).toContain('text-peach')
    expect(svgs[3].getAttribute('class')).toContain('text-gray-300')
  })

  it('clears hover highlight on mouseLeave when not readonly', () => {
    const { container } = render(<StarRating value={0} />)
    const buttons = screen.getAllByRole('button')
    fireEvent.mouseEnter(buttons[2])
    fireEvent.mouseLeave(buttons[2])
    const svgs = container.querySelectorAll('svg')
    svgs.forEach((svg) => expect(svg.getAttribute('class')).toContain('text-gray-300'))
  })

  it('does not change stars on mouseLeave when readonly', () => {
    const { container } = render(<StarRating value={3} readonly />)
    fireEvent.mouseLeave(screen.getAllByRole('button')[2])
    const svgs = container.querySelectorAll('svg')
    expect(svgs[0].getAttribute('class')).toContain('text-peach')
    expect(svgs[4].getAttribute('class')).toContain('text-gray-300')
  })
})
