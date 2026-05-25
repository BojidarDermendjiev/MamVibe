import { describe, it, expect, vi, afterEach } from 'vitest'
import { render, screen, waitFor, act } from '@testing-library/react'
import ShippingPricePreview from './ShippingPricePreview'
import { shippingApi } from '../../api/shippingApi'
import type { CalculateShippingRequest } from '../../types/shipping'
import { CourierProvider, DeliveryType } from '../../types/shipping'

vi.mock('../../api/shippingApi', () => ({
  shippingApi: { calculatePrice: vi.fn() },
}))

const mockCalculatePrice = vi.mocked(shippingApi.calculatePrice)

const request: CalculateShippingRequest = {
  courierProvider: CourierProvider.Econt,
  deliveryType: DeliveryType.Office,
  toCity: 'Sofia',
  weight: 1.0,
  isCod: false,
  codAmount: 0,
  isInsured: false,
  insuredAmount: 0,
}

afterEach(() => {
  mockCalculatePrice.mockClear()
  vi.useRealTimers()
})

describe('ShippingPricePreview', () => {
  it('renders nothing when request is null', () => {
    const { container } = render(<ShippingPricePreview request={null} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders nothing when weight is 0', () => {
    const { container } = render(<ShippingPricePreview request={{ ...request, weight: 0 }} />)
    expect(container.firstChild).toBeNull()
  })

  it('shows price after API resolves', async () => {
    vi.useFakeTimers()
    mockCalculatePrice.mockResolvedValue({ data: { price: 5.9, currency: 'BGN', estimatedDelivery: null } } as never)
    render(<ShippingPricePreview request={request} />)
    await act(async () => {
      vi.advanceTimersByTime(600)
      await Promise.resolve()
      await Promise.resolve()
    })
    vi.useRealTimers()
    await waitFor(() => expect(screen.getByText(/5\.90/)).toBeInTheDocument())
  })

  it('shows estimated delivery when provided', async () => {
    vi.useFakeTimers()
    mockCalculatePrice.mockResolvedValue({ data: { price: 5.9, currency: 'BGN', estimatedDelivery: '2024-01-15' } } as never)
    render(<ShippingPricePreview request={request} />)
    await act(async () => {
      vi.advanceTimersByTime(600)
      await Promise.resolve()
      await Promise.resolve()
    })
    vi.useRealTimers()
    await waitFor(() => expect(screen.getByText(/2024-01-15/)).toBeInTheDocument())
  })

  it('renders nothing when API call fails', async () => {
    vi.useFakeTimers()
    mockCalculatePrice.mockRejectedValue(new Error('Network error'))
    const { container } = render(<ShippingPricePreview request={request} />)
    await act(async () => {
      vi.advanceTimersByTime(600)
      await Promise.resolve()
      await Promise.resolve()
    })
    vi.useRealTimers()
    await waitFor(() => expect(container.firstChild).toBeNull())
  })

  it('calls onPriceChange with price when API succeeds', async () => {
    vi.useFakeTimers()
    mockCalculatePrice.mockResolvedValue({ data: { price: 5.9, currency: 'BGN', estimatedDelivery: null } } as never)
    const onPriceChange = vi.fn()
    render(<ShippingPricePreview request={request} onPriceChange={onPriceChange} />)
    await act(async () => {
      vi.advanceTimersByTime(600)
      await Promise.resolve()
      await Promise.resolve()
    })
    vi.useRealTimers()
    await waitFor(() => expect(onPriceChange).toHaveBeenCalledWith(5.9))
  })

  it('calls onPriceChange with 0 when API fails', async () => {
    vi.useFakeTimers()
    mockCalculatePrice.mockRejectedValue(new Error('Network error'))
    const onPriceChange = vi.fn()
    render(<ShippingPricePreview request={request} onPriceChange={onPriceChange} />)
    await act(async () => {
      vi.advanceTimersByTime(600)
      await Promise.resolve()
      await Promise.resolve()
    })
    vi.useRealTimers()
    await waitFor(() => expect(onPriceChange).toHaveBeenCalledWith(0))
  })

  it('calls onPriceChange with 0 when request is null', () => {
    const onPriceChange = vi.fn()
    render(<ShippingPricePreview request={null} onPriceChange={onPriceChange} />)
    expect(onPriceChange).toHaveBeenCalledWith(0)
  })

  it('calls onPriceChange with 0 when weight is 0', () => {
    const onPriceChange = vi.fn()
    render(<ShippingPricePreview request={{ ...request, weight: 0 }} onPriceChange={onPriceChange} />)
    expect(onPriceChange).toHaveBeenCalledWith(0)
  })
})
