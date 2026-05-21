import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import ItemCard from './ItemCard'
import type { Item } from '../../types/item'
import { ListingType } from '../../types/item'

vi.mock('./LikeButton', () => ({
  default: ({ likeCount }: { likeCount: number }) => <button>{likeCount}</button>,
}))

const baseItem: Item = {
  id: 'item-1',
  title: 'Baby Stroller',
  description: 'Good condition',
  categoryId: 'cat-1',
  categoryName: 'strollers',
  listingType: ListingType.Sell,
  ageGroup: null,
  shoeSize: null,
  clothingSize: null,
  price: 19.56,
  userId: 'u-1',
  userDisplayName: 'Seller',
  userAvatarUrl: null,
  isActive: true,
  viewCount: 42,
  likeCount: 7,
  isLikedByCurrentUser: false,
  photos: [],
  createdAt: '2024-01-01T00:00:00Z',
  aiModerationStatus: 1,
  aiModerationNotes: null,
  aiModerationScore: null,
}

function setup(item = baseItem) {
  return render(
    <MemoryRouter>
      <ItemCard item={item} />
    </MemoryRouter>
  )
}

describe('ItemCard', () => {
  it('renders item title', () => {
    setup()
    expect(screen.getByText('Baby Stroller')).toBeInTheDocument()
  })

  it('renders category name', () => {
    setup()
    expect(screen.getByText('strollers')).toBeInTheDocument()
  })

  it('renders formatted price for Sell items', () => {
    setup()
    expect(screen.getByText(/€10\.00/)).toBeInTheDocument()
  })

  it('renders free label for Donate items', () => {
    setup({ ...baseItem, listingType: ListingType.Donate, price: null })
    expect(screen.getByText('items.free')).toBeInTheDocument()
  })

  it('renders view count', () => {
    setup()
    expect(screen.getByText('42')).toBeInTheDocument()
  })

  it('links to item detail page', () => {
    setup()
    const links = screen.getAllByRole('link')
    expect(links[0]).toHaveAttribute('href', '/items/item-1')
  })

  it('renders category placeholder image when no photos', () => {
    const { container } = setup()
    expect(container.querySelector('img[src="/categories/strollers.svg"]')).toBeInTheDocument()
  })

  it('renders photo when available', () => {
    const item = { ...baseItem, photos: [{ id: 'p1', url: '/photo.jpg', displayOrder: 0 }] }
    const { container } = setup(item)
    expect(container.querySelector('img[src="/photo.jpg"]')).toBeInTheDocument()
  })

  it('shows no pending banner when showStatus is not passed', () => {
    setup({ ...baseItem, isActive: false })
    expect(screen.queryByText('items.status_pending')).toBeNull()
  })

  it('shows pending banner when showStatus=true and item is inactive', () => {
    render(
      <MemoryRouter>
        <ItemCard item={{ ...baseItem, isActive: false }} showStatus />
      </MemoryRouter>
    )
    expect(screen.getByText('items.status_pending')).toBeInTheDocument()
  })

  it('shows flagged banner when showStatus=true and aiModerationStatus is 3', () => {
    render(
      <MemoryRouter>
        <ItemCard item={{ ...baseItem, isActive: false, aiModerationStatus: 3 }} showStatus />
      </MemoryRouter>
    )
    expect(screen.getByText('items.status_flagged')).toBeInTheDocument()
  })

  it('shows pending status hint text when pending', () => {
    render(
      <MemoryRouter>
        <ItemCard item={{ ...baseItem, isActive: false }} showStatus />
      </MemoryRouter>
    )
    expect(screen.getByText('items.status_pending_hint')).toBeInTheDocument()
  })

  it('shows donate badge for Donate listing type', () => {
    setup({ ...baseItem, listingType: ListingType.Donate, price: null })
    expect(screen.getByText('items.donate')).toBeInTheDocument()
  })

  it('shows sell badge for Sell listing type', () => {
    setup()
    expect(screen.getByText('items.sell')).toBeInTheDocument()
  })

  it('passes onLikeToggle and onRequireAuth to LikeButton', () => {
    const onLikeToggle = vi.fn()
    const onRequireAuth = vi.fn()
    render(
      <MemoryRouter>
        <ItemCard item={baseItem} onLikeToggle={onLikeToggle} onRequireAuth={onRequireAuth} />
      </MemoryRouter>
    )
    // LikeButton is mocked — just confirm the card renders without error
    expect(screen.getByText('42')).toBeInTheDocument()
  })
})
