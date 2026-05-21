import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { purchaseRequestsApi } from './purchaseRequestsApi'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
})

describe('purchaseRequestsApi', () => {
  it('create posts itemId to /purchase-requests', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    purchaseRequestsApi.create('item-1')
    expect(client.post).toHaveBeenCalledWith('/purchase-requests', { itemId: 'item-1' })
  })

  it('accept calls POST /purchase-requests/:id/accept', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    purchaseRequestsApi.accept('pr-1')
    expect(client.post).toHaveBeenCalledWith('/purchase-requests/pr-1/accept')
  })

  it('decline calls POST /purchase-requests/:id/decline', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    purchaseRequestsApi.decline('pr-1')
    expect(client.post).toHaveBeenCalledWith('/purchase-requests/pr-1/decline')
  })

  it('paymentChosen calls POST with paymentMethod body', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    purchaseRequestsApi.paymentChosen('pr-1', 'stripe')
    expect(client.post).toHaveBeenCalledWith('/purchase-requests/pr-1/payment-chosen', {
      paymentMethod: 'stripe',
    })
  })

  it('getAsSeller calls GET /purchase-requests/as-seller', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    purchaseRequestsApi.getAsSeller()
    expect(client.get).toHaveBeenCalledWith('/purchase-requests/as-seller')
  })

  it('getAsBuyer calls GET /purchase-requests/as-buyer', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    purchaseRequestsApi.getAsBuyer()
    expect(client.get).toHaveBeenCalledWith('/purchase-requests/as-buyer')
  })

  it('checkBuyer calls GET /purchase-requests/:id/buyer-check', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    purchaseRequestsApi.checkBuyer('pr-1')
    expect(client.get).toHaveBeenCalledWith('/purchase-requests/pr-1/buyer-check')
  })
})
