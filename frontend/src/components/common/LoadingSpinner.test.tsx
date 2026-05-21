import { describe, it, expect } from 'vitest'
import { render } from '@testing-library/react'
import LoadingSpinner from './LoadingSpinner'

describe('LoadingSpinner', () => {
  it('renders with default md size', () => {
    const { container } = render(<LoadingSpinner />)
    expect(container.querySelector('.animate-spin')).toHaveClass('h-8', 'w-8')
  })

  it('renders with sm size', () => {
    const { container } = render(<LoadingSpinner size="sm" />)
    expect(container.querySelector('.animate-spin')).toHaveClass('h-5', 'w-5')
  })

  it('renders with lg size', () => {
    const { container } = render(<LoadingSpinner size="lg" />)
    expect(container.querySelector('.animate-spin')).toHaveClass('h-12', 'w-12')
  })

  it('applies extra className', () => {
    const { container } = render(<LoadingSpinner className="my-custom" />)
    expect(container.firstChild).toHaveClass('my-custom')
  })
})
