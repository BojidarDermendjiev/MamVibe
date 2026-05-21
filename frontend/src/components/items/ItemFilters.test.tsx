import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ItemFilters from './ItemFilters'
import type { ItemFilter, Category } from '../../types/item'
import { ListingType } from '../../types/item'

const defaultFilter: ItemFilter = {
  page: 1,
  pageSize: 12,
  sortBy: 'newest',
}

const categories: Category[] = [
  { id: 'cat-1', name: 'Clothes', description: '', slug: 'clothes' },
  { id: 'cat-2', name: 'Toys', description: '', slug: 'toys' },
]

describe('ItemFilters', () => {
  it('renders category options', () => {
    render(<ItemFilters filter={defaultFilter} categories={categories} onChange={vi.fn()} />)
    expect(screen.getByText('Clothes')).toBeInTheDocument()
    expect(screen.getByText('Toys')).toBeInTheDocument()
  })

  it('calls onChange with category id when category clicked', async () => {
    const onChange = vi.fn()
    render(<ItemFilters filter={defaultFilter} categories={categories} onChange={onChange} />)
    await userEvent.click(screen.getByText('Clothes'))
    expect(onChange).toHaveBeenCalledWith({ categoryId: 'cat-1', page: 1 })
  })

  it('clears category when same category clicked again', async () => {
    const onChange = vi.fn()
    const filter = { ...defaultFilter, categoryId: 'cat-1' }
    render(<ItemFilters filter={filter} categories={categories} onChange={onChange} />)
    await userEvent.click(screen.getByText('Clothes'))
    expect(onChange).toHaveBeenCalledWith({ categoryId: undefined, page: 1 })
  })

  it('shows Clear all button when active filter exists', () => {
    const filter = { ...defaultFilter, categoryId: 'cat-1' }
    render(<ItemFilters filter={filter} categories={categories} onChange={vi.fn()} />)
    expect(screen.getByText('Clear all')).toBeInTheDocument()
  })

  it('does not show Clear all button when no active filters', () => {
    render(<ItemFilters filter={defaultFilter} categories={categories} onChange={vi.fn()} />)
    expect(screen.queryByText('Clear all')).toBeNull()
  })

  it('calls onChange with reset values on Clear all click', async () => {
    const onChange = vi.fn()
    const filter = { ...defaultFilter, categoryId: 'cat-1' }
    render(<ItemFilters filter={filter} categories={categories} onChange={onChange} />)
    await userEvent.click(screen.getByText('Clear all'))
    expect(onChange).toHaveBeenCalledWith({
      categoryId: undefined,
      listingType: undefined,
      brand: undefined,
      ageGroup: undefined,
      sortBy: 'newest',
      page: 1,
    })
  })

  it('calls onChange with Sell listing type when Sell clicked', async () => {
    const onChange = vi.fn()
    render(<ItemFilters filter={defaultFilter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('items.sell'))
    expect(onChange).toHaveBeenCalledWith({ listingType: ListingType.Sell, page: 1 })
  })
})
