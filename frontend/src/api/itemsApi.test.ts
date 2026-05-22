import { describe, it, expect, vi, beforeEach } from 'vitest'
import { itemsApi } from './itemsApi'
import axiosClient from './axiosClient'
import type { ItemFilter } from '../types/item'
import { ListingType } from '../types/item'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
  client.put.mockClear()
  client.delete.mockClear()
})

const filter: ItemFilter = { page: 1, pageSize: 12, sortBy: 'newest' }

describe('itemsApi', () => {
  it('getAll gets /items with filter params', () => {
    itemsApi.getAll(filter)
    expect(client.get).toHaveBeenCalledWith('/items', { params: filter })
  })

  it('getById gets /items/:id', () => {
    itemsApi.getById('abc')
    expect(client.get).toHaveBeenCalledWith('/items/abc')
  })

  it('create posts to /items', () => {
    const req = { title: 'T', description: 'D', categoryId: 'c', listingType: ListingType.Sell, price: 10 }
    itemsApi.create(req)
    expect(client.post).toHaveBeenCalledWith('/items', req)
  })

  it('update puts to /items/:id', () => {
    const req = { title: 'T', description: 'D', categoryId: 'c', listingType: ListingType.Sell, price: 10 }
    itemsApi.update('abc', req)
    expect(client.put).toHaveBeenCalledWith('/items/abc', req)
  })

  it('delete deletes /items/:id', () => {
    itemsApi.delete('abc')
    expect(client.delete).toHaveBeenCalledWith('/items/abc')
  })

  it('toggleLike posts to /items/:id/like', () => {
    itemsApi.toggleLike('abc')
    expect(client.post).toHaveBeenCalledWith('/items/abc/like')
  })

  it('getCategories gets /categories', () => {
    itemsApi.getCategories()
    expect(client.get).toHaveBeenCalledWith('/categories')
  })

  it('getMyItems gets dashboard items endpoint', () => {
    itemsApi.getMyItems()
    expect(client.get).toHaveBeenCalledWith('/users/dashboard/items')
  })

  it('getLikedItems gets dashboard liked endpoint', () => {
    itemsApi.getLikedItems()
    expect(client.get).toHaveBeenCalledWith('/users/dashboard/liked')
  })

  it('incrementView posts to /items/:id/view', () => {
    itemsApi.incrementView('abc')
    expect(client.post).toHaveBeenCalledWith('/items/abc/view')
  })

  it('checkSeller gets /items/:id/seller-check', () => {
    itemsApi.checkSeller('abc')
    expect(client.get).toHaveBeenCalledWith('/items/abc/seller-check')
  })
})
