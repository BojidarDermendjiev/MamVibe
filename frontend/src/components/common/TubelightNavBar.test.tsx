import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { TubelightNavBar, type NavItem } from './TubelightNavBar'
import type { LucideIcon } from 'lucide-react'

const FakeIcon = (() => null) as unknown as LucideIcon

const items: NavItem[] = [
  { name: 'Home', url: '/', icon: FakeIcon },
  { name: 'Browse', url: '/items', icon: FakeIcon },
  { name: 'Messages', url: '/messages', icon: FakeIcon, badge: 3 },
  { name: 'Big Badge', url: '/big', icon: FakeIcon, badge: 15 },
]

function setup(initialPath = '/') {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <TubelightNavBar items={items} />
    </MemoryRouter>
  )
}

describe('TubelightNavBar', () => {
  it('renders all nav item labels', () => {
    setup()
    expect(screen.getByText('Home')).toBeInTheDocument()
    expect(screen.getByText('Browse')).toBeInTheDocument()
    expect(screen.getByText('Messages')).toBeInTheDocument()
  })

  it('links to correct URLs', () => {
    setup()
    const links = screen.getAllByRole('link')
    expect(links[0]).toHaveAttribute('href', '/')
    expect(links[1]).toHaveAttribute('href', '/items')
    expect(links[2]).toHaveAttribute('href', '/messages')
  })

  it('marks Home as active on root path', () => {
    setup('/')
    const homeLink = screen.getByText('Home').closest('a')!
    expect(homeLink.classList.contains('text-primary')).toBe(true)
  })

  it('marks Browse as active when path starts with /items', () => {
    setup('/items/item-123')
    const browseLink = screen.getByText('Browse').closest('a')!
    expect(browseLink.classList.contains('text-primary')).toBe(true)
  })

  it('Home is not active when on /items path', () => {
    setup('/items')
    const homeLink = screen.getByText('Home').closest('a')!
    expect(homeLink.classList.contains('text-primary')).toBe(false)
  })

  it('shows badge count when badge is set', () => {
    setup()
    expect(screen.getAllByText('3').length).toBeGreaterThan(0)
  })

  it('shows 9+ when badge exceeds 9', () => {
    setup()
    expect(screen.getAllByText('9+').length).toBeGreaterThan(0)
  })

  it('does not show badge element when badge is 0', () => {
    const itemsNoBadge = [{ name: 'Home', url: '/', icon: FakeIcon, badge: 0 }]
    render(
      <MemoryRouter initialEntries={['/']}>
        <TubelightNavBar items={itemsNoBadge} />
      </MemoryRouter>
    )
    expect(screen.queryByText('0')).toBeNull()
  })

  it('updates active tab when link is clicked', async () => {
    setup('/')
    await userEvent.click(screen.getByText('Browse'))
    const browseLink = screen.getByText('Browse').closest('a')!
    expect(browseLink.classList.contains('text-primary')).toBe(true)
  })

  it('falls back to first item when no URL matches current path', () => {
    setup('/unknown-path')
    const homeLink = screen.getByText('Home').closest('a')!
    expect(homeLink.classList.contains('text-primary')).toBe(true)
  })
})
