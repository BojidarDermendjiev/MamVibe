import { describe, it, expect, vi, beforeEach } from 'vitest'
import { offersApi } from './offersApi'
import axiosClient from './axiosClient'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
})

describe('offersApi', () => {
  it('create posts to /offers with itemId and offeredPrice', () => {
    offersApi.create('item-1', 75)
    expect(client.post).toHaveBeenCalledWith('/offers', { itemId: 'item-1', offeredPrice: 75 })
  })

  it('accept posts to /offers/:id/accept', () => {
    offersApi.accept('offer-1')
    expect(client.post).toHaveBeenCalledWith('/offers/offer-1/accept')
  })

  it('decline posts to /offers/:id/decline', () => {
    offersApi.decline('offer-1')
    expect(client.post).toHaveBeenCalledWith('/offers/offer-1/decline')
  })

  it('counter posts to /offers/:id/counter with counterPrice', () => {
    offersApi.counter('offer-1', 90)
    expect(client.post).toHaveBeenCalledWith('/offers/offer-1/counter', { counterPrice: 90 })
  })

  it('acceptCounter posts to /offers/:id/accept-counter', () => {
    offersApi.acceptCounter('offer-1')
    expect(client.post).toHaveBeenCalledWith('/offers/offer-1/accept-counter')
  })

  it('declineCounter posts to /offers/:id/decline-counter', () => {
    offersApi.declineCounter('offer-1')
    expect(client.post).toHaveBeenCalledWith('/offers/offer-1/decline-counter')
  })

  it('cancel posts to /offers/:id/cancel', () => {
    offersApi.cancel('offer-1')
    expect(client.post).toHaveBeenCalledWith('/offers/offer-1/cancel')
  })

  it('getReceived gets /offers/received', () => {
    offersApi.getReceived()
    expect(client.get).toHaveBeenCalledWith('/offers/received')
  })

  it('getSent gets /offers/sent', () => {
    offersApi.getSent()
    expect(client.get).toHaveBeenCalledWith('/offers/sent')
  })
})
