import { describe, it, expect, vi, beforeEach } from 'vitest'
import { feedbackApi } from './feedbackApi'
import axiosClient from './axiosClient'
import { FeedbackCategory } from '../types/feedback'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn(), delete: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
  client.delete.mockClear()
})

describe('feedbackApi', () => {
  it('getAll gets /feedback with default pagination', () => {
    feedbackApi.getAll()
    expect(client.get).toHaveBeenCalledWith('/feedback', { params: { page: 1, pageSize: 10 } })
  })

  it('getAll accepts custom page and pageSize', () => {
    feedbackApi.getAll(2, 20)
    expect(client.get).toHaveBeenCalledWith('/feedback', { params: { page: 2, pageSize: 20 } })
  })

  it('create posts to /feedback', () => {
    const req = { rating: 5, category: FeedbackCategory.Praise, content: 'Great!', isContactable: false }
    feedbackApi.create(req)
    expect(client.post).toHaveBeenCalledWith('/feedback', req)
  })

  it('delete deletes /feedback/:id', () => {
    feedbackApi.delete('fb-1')
    expect(client.delete).toHaveBeenCalledWith('/feedback/fb-1')
  })
})
