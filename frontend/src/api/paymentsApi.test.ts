import { describe, it, expect, vi, beforeEach } from 'vitest'
import { paymentsApi } from './paymentsApi'
import axiosClient from './axiosClient'
import { CourierProvider, DeliveryType } from '../types/shipping'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn() },
}))

const client = vi.mocked(axiosClient)

const delivery = {
  courierProvider: CourierProvider.Econt,
  deliveryType: DeliveryType.Office,
  recipientName: 'Maria',
  recipientPhone: '0878000000',
  weight: 1,
}

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
})

describe('paymentsApi', () => {
  it('createCheckout posts to /payments/checkout/:itemId', () => {
    paymentsApi.createCheckout('item-1', delivery)
    expect(client.post).toHaveBeenCalledWith('/payments/checkout/item-1', delivery)
  })

  it('createOnSpot posts to /payments/onspot/:itemId', () => {
    paymentsApi.createOnSpot('item-1', delivery)
    expect(client.post).toHaveBeenCalledWith('/payments/onspot/item-1', delivery)
  })

  it('createBooking posts to /payments/booking/:itemId', () => {
    paymentsApi.createBooking('item-1', delivery)
    expect(client.post).toHaveBeenCalledWith('/payments/booking/item-1', delivery)
  })

  it('getMyPayments gets /payments/my-payments', () => {
    paymentsApi.getMyPayments()
    expect(client.get).toHaveBeenCalledWith('/payments/my-payments')
  })

  it('createPaymentIntent posts to /payments/create-intent/:itemId', () => {
    paymentsApi.createPaymentIntent('item-1')
    expect(client.post).toHaveBeenCalledWith('/payments/create-intent/item-1')
  })

  it('createDonationCheckout posts to /payments/donation/checkout', () => {
    paymentsApi.createDonationCheckout(10)
    expect(client.post).toHaveBeenCalledWith('/payments/donation/checkout', { amount: 10 })
  })
})
