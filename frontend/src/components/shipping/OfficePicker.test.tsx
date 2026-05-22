import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import OfficePicker from './OfficePicker'
import { shippingApi } from '../../api/shippingApi'
import { CourierProvider } from '../../types/shipping'

vi.mock('../../api/shippingApi', () => ({
  shippingApi: { getOffices: vi.fn() },
}))

const mockGetOffices = vi.mocked(shippingApi.getOffices)

const offices = [
  { id: '1', name: 'Sofia Centre', city: 'Sofia', address: '1 Main St', isLocker: false },
  { id: '2', name: 'Sofia Locker', city: 'Sofia', address: '2 Side St', isLocker: true },
  { id: '3', name: 'Plovdiv Office', city: 'Plovdiv', address: '3 Road', isLocker: false },
]

const baseProps = {
  provider: CourierProvider.Econt,
  value: '',
  onChange: vi.fn(),
}

beforeEach(() => {
  mockGetOffices.mockClear()
  baseProps.onChange.mockClear()
})

describe('OfficePicker', () => {
  it('shows spinner while loading', () => {
    mockGetOffices.mockReturnValue(new Promise(() => {}))
    const { container } = render(<OfficePicker {...baseProps} />)
    expect(container.querySelector('.animate-spin')).toBeInTheDocument()
  })

  it('shows search input after loading completes', async () => {
    mockGetOffices.mockResolvedValue({ data: offices } as never)
    render(<OfficePicker {...baseProps} />)
    await waitFor(() => expect(screen.getByPlaceholderText('shipping.filter_offices')).toBeInTheDocument())
  })

  it('opens dropdown on input focus and lists offices', async () => {
    mockGetOffices.mockResolvedValue({ data: offices } as never)
    render(<OfficePicker {...baseProps} />)
    const input = await waitFor(() => screen.getByPlaceholderText('shipping.filter_offices'))
    await userEvent.click(input)
    expect(screen.getByText('Sofia Centre')).toBeInTheDocument()
  })

  it('calls onChange with id and name when office is selected', async () => {
    mockGetOffices.mockResolvedValue({ data: offices } as never)
    render(<OfficePicker {...baseProps} />)
    const input = await waitFor(() => screen.getByPlaceholderText('shipping.filter_offices'))
    await userEvent.click(input)
    await userEvent.click(screen.getByText('Sofia Centre'))
    expect(baseProps.onChange).toHaveBeenCalledWith('1', 'Sofia Centre')
  })

  it('filters offices by search text', async () => {
    mockGetOffices.mockResolvedValue({ data: offices } as never)
    render(<OfficePicker {...baseProps} />)
    const input = await waitFor(() => screen.getByPlaceholderText('shipping.filter_offices'))
    await userEvent.type(input, 'Plovdiv')
    expect(screen.getByText('Plovdiv Office')).toBeInTheDocument()
    expect(screen.queryByText('Sofia Centre')).toBeNull()
  })

  it('shows selected office name when value matches', async () => {
    mockGetOffices.mockResolvedValue({ data: offices } as never)
    render(<OfficePicker {...baseProps} value="1" />)
    await waitFor(() => expect(screen.getByText('Sofia Centre')).toBeInTheDocument())
  })

  it('lockersOnly hides non-locker offices', async () => {
    mockGetOffices.mockResolvedValue({ data: offices } as never)
    render(<OfficePicker {...baseProps} lockersOnly />)
    const input = await waitFor(() => screen.getByPlaceholderText('shipping.filter_offices'))
    await userEvent.click(input)
    expect(screen.queryByText('Sofia Centre')).toBeNull()
    expect(screen.getByText(/Sofia Locker/)).toBeInTheDocument()
  })

  it('closes dropdown on outside click', async () => {
    mockGetOffices.mockResolvedValue({ data: offices } as never)
    render(<OfficePicker {...baseProps} />)
    const input = await waitFor(() => screen.getByPlaceholderText('shipping.filter_offices'))
    await userEvent.click(input)
    expect(screen.getByText('Sofia Centre')).toBeInTheDocument()
    fireEvent.mouseDown(document.body)
    expect(screen.queryByText('Sofia Centre')).toBeNull()
  })

  it('shows empty list when API call fails', async () => {
    mockGetOffices.mockRejectedValue(new Error('Network error'))
    render(<OfficePicker {...baseProps} />)
    const input = await waitFor(() => screen.getByPlaceholderText('shipping.filter_offices'))
    await userEvent.click(input)
    expect(screen.getByText('shipping.choose_office')).toBeInTheDocument()
  })
})
