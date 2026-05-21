import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { userRatingsApi } from './userRatingsApi'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
})

describe('userRatingsApi', () => {
  it('create posts rating to /purchase-requests/:id/rating', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    userRatingsApi.create('pr-1', { rating: 5, comment: 'Great!' })
    expect(client.post).toHaveBeenCalledWith('/purchase-requests/pr-1/rating', {
      rating: 5, comment: 'Great!',
    })
  })

  it('getForUser calls GET /users/:id/ratings', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    userRatingsApi.getForUser('u-1')
    expect(client.get).toHaveBeenCalledWith('/users/u-1/ratings')
  })

  it('getSummary calls GET /users/:id/ratings/summary', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    userRatingsApi.getSummary('u-1')
    expect(client.get).toHaveBeenCalledWith('/users/u-1/ratings/summary')
  })
})
