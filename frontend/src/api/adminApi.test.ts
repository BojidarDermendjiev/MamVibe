import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { adminApi } from './adminApi'

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

describe('adminApi', () => {
  it('getDashboard calls GET /admin/dashboard', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    adminApi.getDashboard()
    expect(client.get).toHaveBeenCalledWith('/admin/dashboard')
  })

  it('getUsers calls GET /admin/users with search param', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    adminApi.getUsers('test')
    expect(client.get).toHaveBeenCalledWith('/admin/users', { params: { search: 'test' } })
  })

  it('getUsers calls GET /admin/users without param', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    adminApi.getUsers()
    expect(client.get).toHaveBeenCalledWith('/admin/users', { params: { search: undefined } })
  })

  it('blockUser calls POST /admin/users/:id/block', () => {
    client.post.mockResolvedValue({} as never)
    adminApi.blockUser('u-1')
    expect(client.post).toHaveBeenCalledWith('/admin/users/u-1/block')
  })

  it('unblockUser calls POST /admin/users/:id/unblock', () => {
    client.post.mockResolvedValue({} as never)
    adminApi.unblockUser('u-1')
    expect(client.post).toHaveBeenCalledWith('/admin/users/u-1/unblock')
  })

  it('deleteItem calls DELETE /admin/items/:id', () => {
    client.delete.mockResolvedValue({} as never)
    adminApi.deleteItem('item-1')
    expect(client.delete).toHaveBeenCalledWith('/admin/items/item-1')
  })

  it('getPendingItems calls GET /admin/items/pending', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    adminApi.getPendingItems()
    expect(client.get).toHaveBeenCalledWith('/admin/items/pending')
  })

  it('approveItem calls POST /admin/items/:id/approve', () => {
    client.post.mockResolvedValue({} as never)
    adminApi.approveItem('item-1')
    expect(client.post).toHaveBeenCalledWith('/admin/items/item-1/approve')
  })

  it('getAllShipments calls GET /admin/shipments', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    adminApi.getAllShipments()
    expect(client.get).toHaveBeenCalledWith('/admin/shipments')
  })

  it('getAllPayments calls GET /admin/payments', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    adminApi.getAllPayments()
    expect(client.get).toHaveBeenCalledWith('/admin/payments')
  })

  it('trackShipment calls GET /admin/shipments/:id/track', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    adminApi.trackShipment('ship-1')
    expect(client.get).toHaveBeenCalledWith('/admin/shipments/ship-1/track')
  })

  it('getModerationHistory calls GET /admin/items/:id/moderation-history', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    adminApi.getModerationHistory('item-1')
    expect(client.get).toHaveBeenCalledWith('/admin/items/item-1/moderation-history')
  })

  it('getAiSettings calls GET /admin/ai-settings', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    adminApi.getAiSettings()
    expect(client.get).toHaveBeenCalledWith('/admin/ai-settings')
  })

  it('updateAiSettings calls PUT /admin/ai-settings with body', () => {
    client.put.mockResolvedValue({ data: {} } as never)
    adminApi.updateAiSettings('gpt-4', 'openai', 'llama3')
    expect(client.put).toHaveBeenCalledWith('/admin/ai-settings', {
      model: 'gpt-4', chatProvider: 'openai', groqModel: 'llama3',
    })
  })

  it('getAuditLogs calls GET /admin/audit-logs with params', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    adminApi.getAuditLogs({ page: 2, pageSize: 20, action: 'login' })
    expect(client.get).toHaveBeenCalledWith('/admin/audit-logs', {
      params: { page: 2, pageSize: 20, action: 'login' },
    })
  })
})
