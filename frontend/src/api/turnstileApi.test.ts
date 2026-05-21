import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { turnstileApi } from './turnstileApi'

vi.mock('./axiosClient', () => ({
  default: { post: vi.fn() },
}))

const mockPost = vi.mocked(axiosClient.post)

beforeEach(() => mockPost.mockClear())

describe('turnstileApi', () => {
  it('verify posts token to /turnstile/verify', () => {
    mockPost.mockResolvedValue({ data: { verified: true } } as never)
    turnstileApi.verify('cf-token-abc')
    expect(mockPost).toHaveBeenCalledWith('/turnstile/verify', { token: 'cf-token-abc' })
  })
})
