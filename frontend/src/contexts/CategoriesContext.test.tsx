import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { CategoriesProvider, useCategories } from './CategoriesContext'
import { itemsApi } from '../api/itemsApi'

vi.mock('../api/itemsApi', () => ({
  itemsApi: { getCategories: vi.fn() },
}))

const mockGetCategories = vi.mocked(itemsApi.getCategories)

function CategoriesConsumer() {
  const { categories, loading } = useCategories()
  if (loading) return <div>Loading...</div>
  return (
    <ul>
      {categories.map((c) => <li key={c.id}>{c.name}</li>)}
    </ul>
  )
}

beforeEach(() => {
  mockGetCategories.mockClear()
})

describe('CategoriesContext', () => {
  it('shows loading initially', () => {
    mockGetCategories.mockReturnValue(new Promise(() => {}) as ReturnType<typeof itemsApi.getCategories>)
    render(<CategoriesProvider><CategoriesConsumer /></CategoriesProvider>)
    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('renders categories after load', async () => {
    mockGetCategories.mockResolvedValue({
      data: [
        { id: 'c1', name: 'Clothes', description: '', slug: 'clothes' },
        { id: 'c2', name: 'Toys', description: '', slug: 'toys' },
      ],
    } as never)
    render(<CategoriesProvider><CategoriesConsumer /></CategoriesProvider>)
    await waitFor(() => {
      expect(screen.getByText('Clothes')).toBeInTheDocument()
      expect(screen.getByText('Toys')).toBeInTheDocument()
    })
  })

  it('renders empty list on API error', async () => {
    mockGetCategories.mockRejectedValue(new Error('Network error'))
    render(<CategoriesProvider><CategoriesConsumer /></CategoriesProvider>)
    await waitFor(() => expect(screen.queryByText('Loading...')).toBeNull())
    expect(screen.queryByRole('listitem')).toBeNull()
  })
})
