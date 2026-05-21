import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import Avatar from './Avatar'
import { ProfileType } from '../../types/auth'

describe('Avatar', () => {
  it('renders img when src is provided', () => {
    render(<Avatar src="https://example.com/photo.jpg" />)
    expect(screen.getByRole('img')).toHaveAttribute('src', 'https://example.com/photo.jpg')
  })

  it('uses profile-type default avatar when no src', () => {
    render(<Avatar profileType={ProfileType.Female} />)
    expect(screen.getByRole('img')).toHaveAttribute('src', '/avatars/mom.svg')
  })

  it('uses dad avatar for Male profileType', () => {
    render(<Avatar profileType={ProfileType.Male} />)
    expect(screen.getByRole('img')).toHaveAttribute('src', '/avatars/dad.svg')
  })

  it('uses family avatar for Family profileType', () => {
    render(<Avatar profileType={ProfileType.Family} />)
    expect(screen.getByRole('img')).toHaveAttribute('src', '/avatars/family.svg')
  })

  it('renders fallback div with family svg when no src or profileType', () => {
    render(<Avatar />)
    expect(screen.getByRole('img')).toHaveAttribute('src', '/avatars/family.svg')
  })

  it('applies sm size class', () => {
    render(<Avatar src="/photo.jpg" size="sm" />)
    expect(screen.getByRole('img')).toHaveClass('h-8', 'w-8')
  })

  it('applies lg size class', () => {
    render(<Avatar src="/photo.jpg" size="lg" />)
    expect(screen.getByRole('img')).toHaveClass('h-16', 'w-16')
  })
})
