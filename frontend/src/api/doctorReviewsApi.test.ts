import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { doctorReviewsApi } from './doctorReviewsApi'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn(), delete: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
  client.delete.mockClear()
})

describe('doctorReviewsApi', () => {
  it('getAll calls GET /doctor-reviews with params', async () => {
    client.get.mockResolvedValue({ data: [] } as never)
    await doctorReviewsApi.getAll({ city: 'Sofia', specialization: 'Pediatrics' })
    expect(client.get).toHaveBeenCalledWith('/doctor-reviews', {
      params: { city: 'Sofia', specialization: 'Pediatrics' },
    })
  })

  it('getById calls GET /doctor-reviews/:id and returns data', async () => {
    client.get.mockResolvedValue({ data: { id: 'dr-1' } } as never)
    const result = await doctorReviewsApi.getById('dr-1')
    expect(client.get).toHaveBeenCalledWith('/doctor-reviews/dr-1')
    expect(result).toEqual({ id: 'dr-1' })
  })

  it('getMine calls GET /doctor-reviews/mine', async () => {
    client.get.mockResolvedValue({ data: [] } as never)
    await doctorReviewsApi.getMine()
    expect(client.get).toHaveBeenCalledWith('/doctor-reviews/mine')
  })

  it('create posts to /doctor-reviews', async () => {
    const dto = { doctorName: 'Dr Smith', city: 'Sofia', specialization: 'GP', rating: 5 } as never
    client.post.mockResolvedValue({ data: dto } as never)
    const result = await doctorReviewsApi.create(dto)
    expect(client.post).toHaveBeenCalledWith('/doctor-reviews', dto)
    expect(result).toEqual(dto)
  })

  it('delete calls DELETE /doctor-reviews/:id', () => {
    client.delete.mockResolvedValue({} as never)
    doctorReviewsApi.delete('dr-1')
    expect(client.delete).toHaveBeenCalledWith('/doctor-reviews/dr-1')
  })

  it('getPending calls GET /admin/doctor-reviews/pending', async () => {
    client.get.mockResolvedValue({ data: [] } as never)
    await doctorReviewsApi.getPending()
    expect(client.get).toHaveBeenCalledWith('/admin/doctor-reviews/pending')
  })

  it('approve calls POST /admin/doctor-reviews/:id/approve', () => {
    client.post.mockResolvedValue({} as never)
    doctorReviewsApi.approve('dr-1')
    expect(client.post).toHaveBeenCalledWith('/admin/doctor-reviews/dr-1/approve')
  })

  it('adminDelete calls DELETE /admin/doctor-reviews/:id', () => {
    client.delete.mockResolvedValue({} as never)
    doctorReviewsApi.adminDelete('dr-1')
    expect(client.delete).toHaveBeenCalledWith('/admin/doctor-reviews/dr-1')
  })
})
