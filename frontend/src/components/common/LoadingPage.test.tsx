import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import LoadingPage from './LoadingPage'

describe('LoadingPage', () => {
  it('renders the MamVibe logo', () => {
    render(<LoadingPage />)
    expect(screen.getByAltText('MamVibe')).toBeInTheDocument()
  })

  it('renders the MamVibe brand name', () => {
    render(<LoadingPage />)
    expect(screen.getByText('MamVibe')).toBeInTheDocument()
  })

  it('renders the loading text', () => {
    render(<LoadingPage />)
    expect(screen.getByText('Loading data...')).toBeInTheDocument()
  })

  it('renders three bouncing dots', () => {
    const { container } = render(<LoadingPage />)
    expect(container.querySelectorAll('.animate-bounce')).toHaveLength(3)
  })

  it('logo has correct src', () => {
    render(<LoadingPage />)
    expect(screen.getByAltText('MamVibe')).toHaveAttribute('src', '/logo.png')
  })
})
