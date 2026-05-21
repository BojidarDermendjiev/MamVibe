import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { aiApi } from './aiApi'

vi.mock('./axiosClient', () => ({
  default: { post: vi.fn() },
}))

const mockPost = vi.mocked(axiosClient.post)

beforeEach(() => mockPost.mockClear())

describe('aiApi', () => {
  it('suggestListing posts to /items/ai-suggest with FormData containing the photo', () => {
    mockPost.mockResolvedValue({ data: {} } as never)
    const file = new File(['img'], 'photo.jpg', { type: 'image/jpeg' })
    aiApi.suggestListing(file)
    expect(mockPost).toHaveBeenCalledOnce()
    const [url, body] = mockPost.mock.calls[0]
    expect(url).toBe('/items/ai-suggest')
    expect(body).toBeInstanceOf(FormData)
    expect((body as FormData).get('photo')).toBe(file)
  })

  it('suggestPrice posts to /items/suggest-price with the request body', () => {
    mockPost.mockResolvedValue({ data: {} } as never)
    const req = { title: 'Baby Jacket', categoryName: 'clothing', condition: 'good' }
    aiApi.suggestPrice(req as never)
    expect(mockPost).toHaveBeenCalledWith('/items/suggest-price', req)
  })
})
