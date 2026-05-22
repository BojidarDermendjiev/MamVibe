import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ItemFilters from './ItemFilters'
import type { ItemFilter, Category } from '../../types/item'
import { AgeGroup, ListingType } from '../../types/item'

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

  it('clears listingType when same type clicked again', async () => {
    const onChange = vi.fn()
    const filter = { ...defaultFilter, listingType: ListingType.Sell }
    render(<ItemFilters filter={filter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('items.sell'))
    expect(onChange).toHaveBeenCalledWith({ listingType: undefined, page: 1 })
  })

  it('calls onChange with Donate listing type when Donate clicked', async () => {
    const onChange = vi.fn()
    render(<ItemFilters filter={defaultFilter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('items.donate'))
    expect(onChange).toHaveBeenCalledWith({ listingType: ListingType.Donate, page: 1 })
  })

  it('selects age group', async () => {
    const onChange = vi.fn()
    render(<ItemFilters filter={defaultFilter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('Toddler'))
    expect(onChange).toHaveBeenCalledWith({ ageGroup: expect.any(Number), page: 1 })
  })

  it('deselects age group when same clicked again', async () => {
    const onChange = vi.fn()
    const filter = { ...defaultFilter, ageGroup: AgeGroup.Toddler }
    render(<ItemFilters filter={filter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('Toddler'))
    expect(onChange).toHaveBeenCalledWith({ ageGroup: undefined, page: 1 })
  })

  it('selects brand', async () => {
    const onChange = vi.fn()
    render(<ItemFilters filter={defaultFilter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('Bugaboo'))
    expect(onChange).toHaveBeenCalledWith({ brand: 'Bugaboo', page: 1 })
  })

  it('deselects brand when same clicked again', async () => {
    const onChange = vi.fn()
    const filter = { ...defaultFilter, brand: 'Bugaboo' }
    render(<ItemFilters filter={filter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('Bugaboo'))
    expect(onChange).toHaveBeenCalledWith({ brand: undefined, page: 1 })
  })

  it('changes sort order', async () => {
    const onChange = vi.fn()
    render(<ItemFilters filter={defaultFilter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('Price: low to high'))
    expect(onChange).toHaveBeenCalledWith({ sortBy: 'price_asc', page: 1 })
  })

  it('shows Clear all when listingType is active', () => {
    render(<ItemFilters filter={{ ...defaultFilter, listingType: ListingType.Sell }} categories={[]} onChange={vi.fn()} />)
    expect(screen.getByText('Clear all')).toBeInTheDocument()
  })

  it('shows Clear all when brand is active', () => {
    render(<ItemFilters filter={{ ...defaultFilter, brand: 'Nuna' }} categories={[]} onChange={vi.fn()} />)
    expect(screen.getByText('Clear all')).toBeInTheDocument()
  })

  it('shows Clear all when ageGroup is active', () => {
    render(<ItemFilters filter={{ ...defaultFilter, ageGroup: 1 }} categories={[]} onChange={vi.fn()} />)
    expect(screen.getByText('Clear all')).toBeInTheDocument()
  })

  it('shows Clear all when sortBy is not newest', () => {
    render(<ItemFilters filter={{ ...defaultFilter, sortBy: 'oldest' }} categories={[]} onChange={vi.fn()} />)
    expect(screen.getByText('Clear all')).toBeInTheDocument()
  })

  it('clears listingType to undefined when All clicked', async () => {
    const onChange = vi.fn()
    const filter = { ...defaultFilter, listingType: ListingType.Sell }
    render(<ItemFilters filter={filter} categories={[]} onChange={onChange} />)
    // "All" is the first option under Listing Type
    const allButtons = screen.getAllByText('All')
    await userEvent.click(allButtons[0])
    expect(onChange).toHaveBeenCalledWith({ listingType: undefined, page: 1 })
  })

  it('calls onChange with categoryId undefined when All categories clicked', async () => {
    const onChange = vi.fn()
    const filter = { ...defaultFilter, categoryId: 'cat-1' }
    render(<ItemFilters filter={filter} categories={categories} onChange={onChange} />)
    await userEvent.click(screen.getByText('items.all_categories'))
    expect(onChange).toHaveBeenCalledWith({ categoryId: undefined, page: 1 })
  })

  it('calls onChange with ageGroup undefined when All ages clicked', async () => {
    const onChange = vi.fn()
    render(<ItemFilters filter={defaultFilter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('All ages'))
    expect(onChange).toHaveBeenCalledWith({ ageGroup: undefined, page: 1 })
  })

  it('calls onChange with brand undefined when All brands clicked', async () => {
    const onChange = vi.fn()
    render(<ItemFilters filter={defaultFilter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('All brands'))
    expect(onChange).toHaveBeenCalledWith({ brand: undefined, page: 1 })
  })

  it('clears listingType when Donate clicked while already selected', async () => {
    const onChange = vi.fn()
    const filter = { ...defaultFilter, listingType: ListingType.Donate }
    render(<ItemFilters filter={filter} categories={[]} onChange={onChange} />)
    await userEvent.click(screen.getByText('items.donate'))
    expect(onChange).toHaveBeenCalledWith({ listingType: undefined, page: 1 })
  })
})
