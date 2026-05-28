import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { bundlesApi } from './bundlesApi'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn(), delete: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
  client.delete.mockClear()
})

describe('bundlesApi', () => {
  it('getById calls GET /bundles/:id', () => {
    client.get.mockResolvedValue({ data: {} } as never)
    bundlesApi.getById('bundle-1')
    expect(client.get).toHaveBeenCalledWith('/bundles/bundle-1')
  })

  it('getMy calls GET /bundles/my', () => {
    client.get.mockResolvedValue({ data: [] } as never)
    bundlesApi.getMy()
    expect(client.get).toHaveBeenCalledWith('/bundles/my')
  })

  it('create posts dto to /bundles', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    const dto = { title: 'Test', description: null, price: 30, itemIds: ['a', 'b'] }
    bundlesApi.create(dto)
    expect(client.post).toHaveBeenCalledWith('/bundles', dto)
  })

  it('delete calls DELETE /bundles/:id', () => {
    client.delete.mockResolvedValue({ data: {} } as never)
    bundlesApi.delete('bundle-1')
    expect(client.delete).toHaveBeenCalledWith('/bundles/bundle-1')
  })

  it('requestPurchase posts to /bundles/:id/request', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    bundlesApi.requestPurchase('bundle-1')
    expect(client.post).toHaveBeenCalledWith('/bundles/bundle-1/request')
  })

  it('createCheckout posts delivery to /bundles/:id/payment/checkout', () => {
    client.post.mockResolvedValue({ data: { url: 'https://stripe.com' } } as never)
    const delivery = { courierProvider: 1, deliveryType: 0, recipientName: 'Jane', recipientPhone: '0888', weight: 1 }
    bundlesApi.createCheckout('bundle-1', delivery as never)
    expect(client.post).toHaveBeenCalledWith('/bundles/bundle-1/payment/checkout', delivery)
  })

  it('createOnSpot posts delivery to /bundles/:id/payment/on-spot', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    bundlesApi.createOnSpot('bundle-1', null)
    expect(client.post).toHaveBeenCalledWith('/bundles/bundle-1/payment/on-spot', null)
  })

  it('createCod posts delivery to /bundles/:id/payment/cod', () => {
    client.post.mockResolvedValue({ data: {} } as never)
    const delivery = { courierProvider: 1, deliveryType: 0, recipientName: 'Jane', recipientPhone: '0888', weight: 1 }
    bundlesApi.createCod('bundle-1', delivery as never)
    expect(client.post).toHaveBeenCalledWith('/bundles/bundle-1/payment/cod', delivery)
  })
})
