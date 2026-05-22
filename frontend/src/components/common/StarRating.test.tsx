import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
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

  it('highlights stars up to hovered position on mouseEnter', () => {
    const { container } = render(<StarRating value={0} />)
    const buttons = screen.getAllByRole('button')
    fireEvent.mouseEnter(buttons[2]) // hover star 3
    const svgs = container.querySelectorAll('svg')
    expect(svgs[0].getAttribute('class')).toContain('text-yellow-400')
    expect(svgs[2].getAttribute('class')).toContain('text-yellow-400')
    expect(svgs[3].getAttribute('class')).toContain('text-gray-300')
  })

  it('clears hover highlight on mouseLeave', () => {
    const { container } = render(<StarRating value={0} />)
    const buttons = screen.getAllByRole('button')
    fireEvent.mouseEnter(buttons[2])
    fireEvent.mouseLeave(buttons[2])
    const svgs = container.querySelectorAll('svg')
    svgs.forEach((svg) => {
      expect(svg.getAttribute('class')).toContain('text-gray-300')
    })
  })

  it('does not highlight on hover when readonly', () => {
    const { container } = render(<StarRating value={0} readonly />)
    const buttons = screen.getAllByRole('button')
    fireEvent.mouseEnter(buttons[2])
    const svgs = container.querySelectorAll('svg')
    svgs.forEach((svg) => {
      expect(svg.getAttribute('class')).toContain('text-gray-300')
    })
  })
})
