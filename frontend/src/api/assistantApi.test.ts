import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { assistantApi } from './assistantApi'

vi.mock('./axiosClient', () => ({
  default: { post: vi.fn() },
}))

const mockPost = vi.mocked(axiosClient.post)

beforeEach(() => mockPost.mockClear())

describe('assistantApi', () => {
  it('chat posts message, history, and language to /assistant/chat', () => {
    mockPost.mockResolvedValue({ data: { reply: 'Hello!' } } as never)
    const history = [{ role: 'user' as const, content: 'Hi' }]
    assistantApi.chat('What is this?', history, 'bg')
    expect(mockPost).toHaveBeenCalledWith('/assistant/chat', {
      message: 'What is this?',
      history,
      language: 'bg',
    })
  })

  it('chat with empty history', () => {
    mockPost.mockResolvedValue({ data: { reply: 'Hi!' } } as never)
    assistantApi.chat('Hello', [], 'en')
    expect(mockPost).toHaveBeenCalledWith('/assistant/chat', {
      message: 'Hello',
      history: [],
      language: 'en',
    })
  })
})
