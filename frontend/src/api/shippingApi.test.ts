import { describe, it, expect, vi, beforeEach } from 'vitest'
import { shippingApi } from './shippingApi'
import axiosClient from './axiosClient'
import { CourierProvider, DeliveryType } from '../types/shipping'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
})

describe('shippingApi', () => {
  it('calculatePrice posts to /shipping/calculate', () => {
    const req = { courierProvider: CourierProvider.Econt, deliveryType: DeliveryType.Office, weight: 1, isCod: false, codAmount: 0, isInsured: false, insuredAmount: 0 }
    shippingApi.calculatePrice(req)
    expect(client.post).toHaveBeenCalledWith('/shipping/calculate', req)
  })

  it('getLabel gets /shipping/:id/label as blob', () => {
    shippingApi.getLabel('ship-1')
    expect(client.get).toHaveBeenCalledWith('/shipping/ship-1/label', { responseType: 'blob' })
  })

  it('trackShipment gets /shipping/:id/track', () => {
    shippingApi.trackShipment('ship-1')
    expect(client.get).toHaveBeenCalledWith('/shipping/ship-1/track')
  })

  it('cancelShipment posts to /shipping/:id/cancel', () => {
    shippingApi.cancelShipment('ship-1')
    expect(client.post).toHaveBeenCalledWith('/shipping/ship-1/cancel')
  })

  it('getOffices gets /shipping/offices with params', () => {
    shippingApi.getOffices(CourierProvider.Speedy, 'Sofia')
    expect(client.get).toHaveBeenCalledWith('/shipping/offices', { params: { provider: CourierProvider.Speedy, city: 'Sofia' } })
  })

  it('getMyShipments gets /shipping/my-shipments', () => {
    shippingApi.getMyShipments()
    expect(client.get).toHaveBeenCalledWith('/shipping/my-shipments')
  })

  it('getShipmentByPayment gets /shipping/payment/:paymentId', () => {
    shippingApi.getShipmentByPayment('pay-1')
    expect(client.get).toHaveBeenCalledWith('/shipping/payment/pay-1')
  })
})
