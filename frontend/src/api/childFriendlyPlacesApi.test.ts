import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { childFriendlyPlacesApi } from './childFriendlyPlacesApi'
import { PlaceType } from '../types/childFriendlyPlace'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn(), delete: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
  client.delete.mockClear()
})

describe('childFriendlyPlacesApi', () => {
  it('getAll calls GET /child-friendly-places with params', async () => {
    client.get.mockResolvedValue({ data: [] } as never)
    await childFriendlyPlacesApi.getAll({ city: 'Sofia', page: 1 })
    expect(client.get).toHaveBeenCalledWith('/child-friendly-places', {
      params: { city: 'Sofia', page: 1 },
    })
  })

  it('getById calls GET /child-friendly-places/:id', async () => {
    client.get.mockResolvedValue({ data: { id: 'p-1' } } as never)
    const result = await childFriendlyPlacesApi.getById('p-1')
    expect(client.get).toHaveBeenCalledWith('/child-friendly-places/p-1')
    expect(result).toEqual({ id: 'p-1' })
  })

  it('create posts to /child-friendly-places', async () => {
    const dto = { name: 'Park', city: 'Sofia', placeType: PlaceType.Park } as never
    client.post.mockResolvedValue({ data: dto } as never)
    const result = await childFriendlyPlacesApi.create(dto)
    expect(client.post).toHaveBeenCalledWith('/child-friendly-places', dto)
    expect(result).toEqual(dto)
  })

  it('delete calls DELETE /child-friendly-places/:id', () => {
    client.delete.mockResolvedValue({} as never)
    childFriendlyPlacesApi.delete('p-1')
    expect(client.delete).toHaveBeenCalledWith('/child-friendly-places/p-1')
  })

  it('getPending calls GET /admin/child-friendly-places/pending', async () => {
    client.get.mockResolvedValue({ data: [] } as never)
    await childFriendlyPlacesApi.getPending()
    expect(client.get).toHaveBeenCalledWith('/admin/child-friendly-places/pending')
  })

  it('approve calls POST /admin/child-friendly-places/:id/approve', () => {
    client.post.mockResolvedValue({} as never)
    childFriendlyPlacesApi.approve('p-1')
    expect(client.post).toHaveBeenCalledWith('/admin/child-friendly-places/p-1/approve')
  })

  it('adminDelete calls DELETE /admin/child-friendly-places/:id', () => {
    client.delete.mockResolvedValue({} as never)
    childFriendlyPlacesApi.adminDelete('p-1')
    expect(client.delete).toHaveBeenCalledWith('/admin/child-friendly-places/p-1')
  })
})
