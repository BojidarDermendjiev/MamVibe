import { describe, it, expect, vi, beforeEach } from 'vitest'
import { messagesApi } from './messagesApi'
import axiosClient from './axiosClient'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
  client.put.mockClear()
})

describe('messagesApi', () => {
  it('getConversations gets /messages/conversations', () => {
    messagesApi.getConversations()
    expect(client.get).toHaveBeenCalledWith('/messages/conversations')
  })

  it('getMessages gets /messages/:userId', () => {
    messagesApi.getMessages('user-1')
    expect(client.get).toHaveBeenCalledWith('/messages/user-1')
  })

  it('send posts to /messages with receiverId and content', () => {
    messagesApi.send('user-1', 'Hello')
    expect(client.post).toHaveBeenCalledWith('/messages', { receiverId: 'user-1', content: 'Hello' })
  })

  it('markAsRead puts to /messages/:userId/read', () => {
    messagesApi.markAsRead('user-1')
    expect(client.put).toHaveBeenCalledWith('/messages/user-1/read')
  })
})
