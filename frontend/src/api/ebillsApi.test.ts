import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { ebillsApi } from './ebillsApi'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
})

describe('ebillsApi', () => {
  it('getMyEBills calls GET /ebills', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    ebillsApi.getMyEBills()
    expect(client.get).toHaveBeenCalledWith('/ebills')
  })

  it('getEBill calls GET /ebills/:id', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    ebillsApi.getEBill('bill-1')
    expect(client.get).toHaveBeenCalledWith('/ebills/bill-1')
  })

  it('resendEmail calls POST /ebills/:id/resend', () => {
    client.post.mockResolvedValue({} as never)
    ebillsApi.resendEmail('bill-1')
    expect(client.post).toHaveBeenCalledWith('/ebills/bill-1/resend')
  })
})
