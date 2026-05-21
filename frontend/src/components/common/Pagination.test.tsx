import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Pagination from './Pagination'

describe('Pagination', () => {
  it('renders nothing when totalPages is 1', () => {
    const { container } = render(
      <Pagination currentPage={1} totalPages={1} onPageChange={vi.fn()} />
    )
    expect(container.firstChild).toBeNull()
  })

  it('renders nothing when totalPages is 0', () => {
    const { container } = render(
      <Pagination currentPage={1} totalPages={0} onPageChange={vi.fn()} />
    )
    expect(container.firstChild).toBeNull()
  })

  it('renders all page buttons when count is small enough to fit without ellipsis', () => {
    // currentPage=2, totalPages=3 → window covers all pages: no ellipsis
    render(<Pagination currentPage={2} totalPages={3} onPageChange={vi.fn()} />)
    ;[1, 2, 3].forEach((n) => {
      expect(screen.getByRole('button', { name: String(n) })).toBeInTheDocument()
    })
  })

  it('calls onPageChange with next page on right arrow click', async () => {
    const handler = vi.fn()
    render(<Pagination currentPage={2} totalPages={5} onPageChange={handler} />)
    const buttons = screen.getAllByRole('button')
    await userEvent.click(buttons[buttons.length - 1]) // right arrow is last
    expect(handler).toHaveBeenCalledWith(3)
  })

  it('calls onPageChange with previous page on left arrow click', async () => {
    const handler = vi.fn()
    render(<Pagination currentPage={3} totalPages={5} onPageChange={handler} />)
    const buttons = screen.getAllByRole('button')
    await userEvent.click(buttons[0]) // left arrow is first
    expect(handler).toHaveBeenCalledWith(2)
  })

  it('disables left arrow on first page', () => {
    render(<Pagination currentPage={1} totalPages={5} onPageChange={vi.fn()} />)
    expect(screen.getAllByRole('button')[0]).toBeDisabled()
  })

  it('disables right arrow on last page', () => {
    render(<Pagination currentPage={5} totalPages={5} onPageChange={vi.fn()} />)
    const buttons = screen.getAllByRole('button')
    expect(buttons[buttons.length - 1]).toBeDisabled()
  })

  it('calls onPageChange with correct page on page button click', async () => {
    const handler = vi.fn()
    // currentPage=3 renders pages 1-5 without ellipsis (window 2-4 covers all interior pages)
    render(<Pagination currentPage={3} totalPages={5} onPageChange={handler} />)
    await userEvent.click(screen.getByRole('button', { name: '3' }))
    expect(handler).toHaveBeenCalledWith(3)
  })

  it('shows ellipsis for large page counts', () => {
    render(<Pagination currentPage={1} totalPages={20} onPageChange={vi.fn()} />)
    expect(screen.getAllByText('...')).toHaveLength(1)
  })
})
